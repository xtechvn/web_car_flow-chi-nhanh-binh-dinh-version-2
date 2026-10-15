using Entities.ViewModels.Car;
using MongoDB.Bson;
using MongoDB.Driver;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.CMS.Services
{
    public class MissingRegistration
    {
        public string MongoId { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GPLX { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string Camp { get; set; } = string.Empty;
        public string ZaloStatus { get; set; } = string.Empty;
    }

    public class RecoveryPeriod
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class RecoveryPushResult
    {
        public int Pushed { get; set; }
        public int Skipped { get; set; }
        public List<string> Failed { get; set; } = new();
        public int Remaining { get; set; }
    }

    /// <summary>
    /// Đối chiếu xe đăng ký trong MongoDB (bản ghi FE lưu ngay khi cấp số) với VehicleInspection trong SQL.
    /// Xe có trong Mongo mà chưa có trong SQL (n8n/consumer lỗi, message mất) là xe "bị miss", cho phép đẩy lại.
    /// Chỉ xét đợt hiện tại: từ lần reset số gần nhất (TIME_RESET - 5 phút) tới lần reset kế tiếp.
    /// </summary>
    public class RegistrationRecoveryService
    {
        private const int CounterResetOffsetMinutes = 5;
        private static readonly TimeSpan DefaultResetTime = new TimeSpan(17, 55, 0);

        private readonly IConfiguration _configuration;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly RegistrationImportService _importService;
        private readonly MongoClient _mongoClient;

        public RegistrationRecoveryService(
            IConfiguration configuration,
            IAllCodeRepository allCodeRepository,
            RegistrationImportService importService)
        {
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
            _importService = importService;
            string url = "mongodb://" + configuration["MongoServer:user"] + ":" + configuration["MongoServer:pwd"]
                         + "@" + configuration["MongoServer:Host"] + ":" + configuration["MongoServer:Port"];
            _mongoClient = new MongoClient(url);
        }

        public async Task<RecoveryPeriod> GetCurrentPeriodAsync()
        {
            var resetTime = DefaultResetTime;
            var timeReset = await _allCodeRepository.GetListSortByName(AllCodeType.TIME_RESET);
            if (timeReset != null && timeReset.Count > 0 && timeReset[0].UpdateTime.HasValue)
            {
                var t = timeReset[0].UpdateTime.Value;
                resetTime = new TimeSpan(t.Hour, t.Minute, 0);
            }

            var now = DateTime.Now;
            var periodEnd = now.Date.Add(resetTime).AddMinutes(-CounterResetOffsetMinutes);
            if (now >= periodEnd) periodEnd = periodEnd.AddDays(1);
            return new RecoveryPeriod { PeriodStart = periodEnd.AddDays(-1), PeriodEnd = periodEnd };
        }

        public async Task<List<MissingRegistration>> GetMissingAsync()
        {
            var period = await GetCurrentPeriodAsync();
            var mongoItems = await GetMongoItemsAsync(period);
            if (mongoItems.Count == 0) return mongoItems;

            // Nới 12 giờ mỗi phía để bắt cả xe CMS đã sửa giờ đăng ký
            var sqlItems = await _importService.GetExistingAsync(period.PeriodStart.AddHours(-12), period.PeriodEnd.AddHours(12));
            return mongoItems
                .Where(m => !sqlItems.Any(s => RegistrationImportService.IsSameRegistration(s, m.QueueNumber, m.PlateNumber, m.RegistrationTime)))
                .OrderBy(m => m.QueueNumber)
                .ThenBy(m => m.RegistrationTime)
                .ToList();
        }

        public async Task<RecoveryPushResult> PushAsync(List<string> mongoIds)
        {
            var result = new RecoveryPushResult();
            var selected = new HashSet<string>(mongoIds ?? new List<string>());

            // Danh sách luôn tính lại ở server, không tin dữ liệu từ trình duyệt
            var missing = await GetMissingAsync();
            foreach (var item in missing.Where(m => selected.Contains(m.MongoId)))
            {
                try
                {
                    var record = new RegistrationRecord
                    {
                        PlateNumber = (item.PlateNumber ?? string.Empty).Trim().ToUpper(),
                        PhoneNumber = item.PhoneNumber?.Trim() ?? string.Empty,
                        Name = item.Name?.Trim() ?? string.Empty,
                        GPLX = item.GPLX?.Trim() ?? string.Empty,
                        Referee = item.Referee?.Trim() ?? string.Empty,
                        Camp = item.Camp?.Trim() ?? string.Empty,
                        QueueNumber = item.QueueNumber,
                        RegistrationTime = item.RegistrationTime,
                        ZaloStatus = item.ZaloStatus ?? string.Empty
                    };

                    var import = await _importService.ImportIfNotExistsAsync(record);
                    if (!import.Success)
                    {
                        result.Failed.Add(item.PlateNumber + " (số " + item.QueueNumber + ")");
                        LogHelper.InsertLogTelegram($"RegistrationRecovery - Insert SQL lỗi: {item.PlateNumber} - số {item.QueueNumber} - Mongo {item.MongoId}");
                    }
                    else if (import.AlreadyExists)
                    {
                        result.Skipped++;
                    }
                    else
                    {
                        result.Pushed++;
                    }
                }
                catch (Exception ex)
                {
                    result.Failed.Add(item.PlateNumber + " (số " + item.QueueNumber + ")");
                    LogHelper.InsertLogTelegram("RegistrationRecovery - PushAsync: " + item.MongoId + " - " + ex);
                }
            }

            result.Remaining = (await GetMissingAsync()).Count;
            return result;
        }

        private async Task<List<MissingRegistration>> GetMongoItemsAsync(RecoveryPeriod period)
        {
            var db = _mongoClient.GetDatabase(_configuration["MongoServer:catalog_log"]);
            var collection = db.GetCollection<BsonDocument>(_configuration["MongoServer:Data_Car"]);

            var filter = Builders<BsonDocument>.Filter.Gte("RegistrationTime", period.PeriodStart.ToUniversalTime())
                         & Builders<BsonDocument>.Filter.Lt("RegistrationTime", period.PeriodEnd.ToUniversalTime());
            var docs = await collection.Find(filter).ToListAsync();

            var list = new List<MissingRegistration>();
            foreach (var doc in docs)
            {
                if (!doc.TryGetValue("RegistrationTime", out var timeValue) || !timeValue.IsValidDateTime) continue;
                int queueNumber = doc.TryGetValue("QueueNumber", out var q) && q.IsNumeric ? q.ToInt32() : 0;
                if (queueNumber <= 0) continue;

                list.Add(new MissingRegistration
                {
                    MongoId = doc.GetValue("_id", BsonNull.Value).ToString() ?? string.Empty,
                    QueueNumber = queueNumber,
                    RegistrationTime = timeValue.ToUniversalTime().ToLocalTime(),
                    PlateNumber = GetString(doc, "PlateNumber"),
                    Name = GetString(doc, "Name"),
                    GPLX = GetString(doc, "GPLX"),
                    PhoneNumber = GetString(doc, "PhoneNumber"),
                    Referee = GetString(doc, "Referee"),
                    Camp = GetString(doc, "Camp"),
                    ZaloStatus = GetString(doc, "ZaloStatus")
                });
            }
            return list;
        }

        private static string GetString(BsonDocument doc, string name)
        {
            return doc.TryGetValue(name, out var v) && !v.IsBsonNull ? v.ToString() ?? string.Empty : string.Empty;
        }
    }
}
