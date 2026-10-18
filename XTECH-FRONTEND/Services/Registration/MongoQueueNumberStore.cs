using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace XTECH_FRONTEND.Services.Registration
{
    /// <summary>
    /// Fallback cấp số khi Redis lỗi: counter atomic trên Mongo + khóa biển số.
    /// Dùng chung mốc với Redis để khi Redis sống lại có thể sync tiếp tục (không nhảy/trùng).
    /// </summary>
    public class MongoQueueNumberStore
    {
        private const string CounterCollection = "RegistrationCounters";
        private const string LockCollection = "RegistrationPlateLocks";

        private readonly IMongoDatabase _db;
        private readonly string _registrationCollection;
        private readonly RegistrationOptions _options;
        private readonly ILogger<MongoQueueNumberStore> _logger;

        public MongoQueueNumberStore(
            IConfiguration configuration,
            IOptions<RegistrationOptions> options,
            ILogger<MongoQueueNumberStore> logger)
        {
            _options = options.Value;
            _logger = logger;
            var url = "mongodb://" + configuration["MongoServer:user"] + ":" + configuration["MongoServer:pwd"]
                      + "@" + configuration["MongoServer:Host"] + ":" + configuration["MongoServer:Port"];
            var client = new MongoClient(url);
            _db = client.GetDatabase(configuration["MongoServer:catalog_log"]);
            _registrationCollection = configuration["MongoServer:Data_Car"] ?? "Data_Car_Binh_Duong";
        }

        private string CounterId(DateTime businessDate) =>
            $"counter:{_options.Site}:{businessDate:yyyyMMdd}";

        private string LockId(string plateKey, DateTime businessDate) =>
            $"lock:{_options.Site}:{plateKey}:{businessDate:yyyyMMdd}";

        /// <summary>
        /// Mốc sàn để sync Redis: max(counter Mongo, max QueueNumber trong đợt đăng ký).
        /// </summary>
        public async Task<int> GetFloorAsync(RegistrationWindow window)
        {
            var counter = await GetCounterValueAsync(window.BusinessDate);
            var maxReg = await GetMaxQueueNumberFromRegistrationsAsync(window);
            return Math.Max(counter, maxReg);
        }

        public async Task<int> GetCounterValueAsync(DateTime businessDate)
        {
            var col = _db.GetCollection<BsonDocument>(CounterCollection);
            var doc = await col.Find(Builders<BsonDocument>.Filter.Eq("_id", CounterId(businessDate)))
                .FirstOrDefaultAsync();
            if (doc == null) return 0;
            return doc.TryGetValue("value", out var v) && v.IsNumeric ? v.ToInt32() : 0;
        }

        public async Task<int> GetMaxQueueNumberFromRegistrationsAsync(RegistrationWindow window)
        {
            try
            {
                var col = _db.GetCollection<BsonDocument>(_registrationCollection);
                var filter = Builders<BsonDocument>.Filter.Gte("RegistrationTime", window.PeriodStart)
                             & Builders<BsonDocument>.Filter.Lt("RegistrationTime", window.PeriodEnd);
                var sort = Builders<BsonDocument>.Sort.Descending("QueueNumber");
                var doc = await col.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync();
                if (doc == null) return 0;
                return doc.TryGetValue("QueueNumber", out var q) && q.IsNumeric ? q.ToInt32() : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không đọc được max QueueNumber từ Mongo");
                return 0;
            }
        }

        /// <summary>Khóa biển số đang hiệu lực (do fallback Mongo trước đó) — tránh Redis cấp số mới khi vừa sống lại.</summary>
        public async Task<AllocationResult?> TryGetActiveLockAsync(string plateNumber, DateTime businessDate)
        {
            var plateKey = RedisQueueNumberAllocator.NormalizePlate(plateNumber);
            var lockCol = _db.GetCollection<BsonDocument>(LockCollection);
            var doc = await lockCol.Find(Builders<BsonDocument>.Filter.Eq("_id", LockId(plateKey, businessDate)))
                .FirstOrDefaultAsync();
            if (doc == null) return null;
            if (!doc.TryGetValue("expiresAt", out var expVal) || expVal.ToUniversalTime() <= DateTime.UtcNow)
                return null;
            var existingNumber = doc.TryGetValue("queueNumber", out var qn) && qn.IsNumeric ? qn.ToInt32() : 0;
            var remaining = (int)Math.Max(0, (expVal.ToUniversalTime() - DateTime.UtcNow).TotalSeconds);
            return new AllocationResult
            {
                Status = AllocationStatus.AlreadyRegistered,
                QueueNumber = existingNumber,
                RemainingSeconds = remaining
            };
        }

        /// <summary>
        /// Cấp số atomic khi Redis không dùng được. Đồng thời ghi counter Mongo để Redis sync sau.
        /// </summary>
        public async Task<AllocationResult> AllocateAsync(string plateNumber, RegistrationWindow window)
        {
            var plateKey = RedisQueueNumberAllocator.NormalizePlate(plateNumber);
            var lockId = LockId(plateKey, window.BusinessDate);
            var lockCol = _db.GetCollection<BsonDocument>(LockCollection);
            var now = DateTime.UtcNow;
            var lockTtl = TimeSpan.FromMinutes(_options.LockMinutes);

            var existing = await TryGetActiveLockAsync(plateNumber, window.BusinessDate);
            if (existing != null) return existing;

            // Atomic: value = max(value hiện tại, max đăng ký đợt, floor counter) + 1
            var floor = await GetFloorAsync(window);
            var counterCol = _db.GetCollection<BsonDocument>(CounterCollection);
            var counterId = CounterId(window.BusinessDate);

            var pipeline = new EmptyPipelineDefinition<BsonDocument>()
                .AppendStage<BsonDocument, BsonDocument, BsonDocument>(new BsonDocument("$set", new BsonDocument
                {
                    {
                        "value", new BsonDocument("$add", new BsonArray
                        {
                            new BsonDocument("$max", new BsonArray
                            {
                                new BsonDocument("$ifNull", new BsonArray { "$value", 0 }),
                                floor
                            }),
                            1
                        })
                    },
                    { "updatedAt", "$$NOW" }
                }));

            var updated = await counterCol.FindOneAndUpdateAsync(
                Builders<BsonDocument>.Filter.Eq("_id", counterId),
                pipeline,
                new FindOneAndUpdateOptions<BsonDocument>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                });

            var queueNumber = updated != null && updated.TryGetValue("value", out var v) && v.IsNumeric
                ? v.ToInt32()
                : floor + 1;

            await lockCol.ReplaceOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", lockId),
                new BsonDocument
                {
                    { "_id", lockId },
                    { "queueNumber", queueNumber },
                    { "plate", plateKey },
                    { "expiresAt", now.Add(lockTtl) },
                    { "updatedAt", now }
                },
                new ReplaceOptions { IsUpsert = true });

            return new AllocationResult
            {
                Status = AllocationStatus.Allocated,
                QueueNumber = queueNumber,
                RemainingSeconds = (int)lockTtl.TotalSeconds
            };
        }

        /// <summary>Đảm bảo counter Mongo ≥ số vừa cấp từ Redis (để fallback sau không thấp hơn).</summary>
        public async Task EnsureCounterAtLeastAsync(DateTime businessDate, int value)
        {
            if (value <= 0) return;
            var col = _db.GetCollection<BsonDocument>(CounterCollection);
            await col.UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq("_id", CounterId(businessDate)),
                Builders<BsonDocument>.Update
                    .Max("value", value)
                    .Set("updatedAt", DateTime.UtcNow)
                    .SetOnInsert("_id", CounterId(businessDate)),
                new UpdateOptions { IsUpsert = true });
        }
    }
}
