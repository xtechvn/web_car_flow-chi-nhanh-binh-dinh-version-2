using Microsoft.Extensions.Caching.Memory;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.Registration
{
    /// <summary>
    /// Cấp số: ưu tiên Redis (atomic). Redis lỗi → Mongo counter atomic.
    /// Khi Redis sống lại: sync floor từ Mongo rồi INCR tiếp (không mất/trùng số).
    /// </summary>
    public class ResilientQueueNumberAllocator : IQueueNumberAllocator
    {
        private static readonly TimeSpan FloorCacheTtl = TimeSpan.FromSeconds(3);

        private readonly RedisQueueNumberAllocator _redisAllocator;
        private readonly MongoQueueNumberStore _mongoStore;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResilientQueueNumberAllocator> _logger;

        public ResilientQueueNumberAllocator(
            RedisQueueNumberAllocator redisAllocator,
            MongoQueueNumberStore mongoStore,
            IMemoryCache cache,
            ILogger<ResilientQueueNumberAllocator> logger)
        {
            _redisAllocator = redisAllocator;
            _mongoStore = mongoStore;
            _cache = cache;
            _logger = logger;
        }

        public async Task<AllocationResult> AllocateAsync(
            string plateNumber,
            RegistrationWindow window,
            string clientIp)
        {
            // Khóa biển từ đợt fallback Mongo (Redis vừa sống lại vẫn tôn trọng)
            var mongoLock = await _mongoStore.TryGetActiveLockAsync(plateNumber, window.BusinessDate);
            if (mongoLock != null) return mongoLock;

            var floor = await GetFloorCachedAsync(window);

            try
            {
                if (!_redisAllocator.IsConnected)
                {
                    throw new RedisUnavailableException("Redis IsConnected=false");
                }

                var result = await _redisAllocator.AllocateAsync(
                    plateNumber, window.BusinessDate, clientIp, floor);

                // Redis đã cấp số mới → cập nhật cache floor để lần sau sync đúng
                if (result.Status == AllocationStatus.Allocated)
                {
                    _cache.Set(FloorCacheKey(window), result.QueueNumber, FloorCacheTtl);
                    // Best-effort: đẩy counter Mongo theo Redis để fallback sau này không thấp hơn
                    _ = Task.Run(() => SyncMongoCounterFireAndForget(window, result.QueueNumber));
                }

                return result;
            }
            catch (Exception ex) when (IsRedisFailure(ex))
            {
                _logger.LogWarning(ex, "Redis cấp số lỗi → fallback Mongo (biển {Plate}, đợt {Date})",
                    plateNumber, window.BusinessDate.ToString("yyyy-MM-dd"));
                _ = Task.Run(() => LogHelper.InsertLogTelegram(
                    $"ResilientQueueNumberAllocator - Redis lỗi, fallback Mongo: {ex.Message}"));

                var fallback = await _mongoStore.AllocateAsync(plateNumber, window);
                if (fallback.Status == AllocationStatus.Allocated)
                {
                    _cache.Set(FloorCacheKey(window), fallback.QueueNumber, FloorCacheTtl);
                }
                return fallback;
            }
        }

        private async Task<int> GetFloorCachedAsync(RegistrationWindow window)
        {
            var key = FloorCacheKey(window);
            if (_cache.TryGetValue(key, out int cached))
            {
                return cached;
            }

            try
            {
                var floor = await _mongoStore.GetFloorAsync(window);
                _cache.Set(key, floor, FloorCacheTtl);
                return floor;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không lấy được floor Mongo, dùng 0");
                return 0;
            }
        }

        private async Task SyncMongoCounterFireAndForget(RegistrationWindow window, int queueNumber)
        {
            try
            {
                // AllocateAsync Mongo đã Max+Inc; ở đây chỉ đảm bảo counter >= số Redis vừa cấp
                // bằng cách gọi GetFloor rồi nếu thấp hơn thì Allocate path Max — đơn giản: Max qua Update
                var current = await _mongoStore.GetCounterValueAsync(window.BusinessDate);
                if (queueNumber <= current) return;

                // Dùng Allocate path không phù hợp (sẽ +1). Gọi lại GetFloor sau Max bằng Update trong store.
                // Tái sử dụng: gọi GetFloorAsync không đủ. Thêm EnsureCounterAtLeast trong store.
                await _mongoStore.EnsureCounterAtLeastAsync(window.BusinessDate, queueNumber);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Sync Mongo counter sau Redis allocate thất bại (không ảnh hưởng user)");
            }
        }

        private static string FloorCacheKey(RegistrationWindow window) =>
            $"reg:floor:{window.BusinessDate:yyyyMMdd}";

        private static bool IsRedisFailure(Exception ex)
        {
            if (ex is RedisUnavailableException) return true;
            var name = ex.GetType().FullName ?? ex.GetType().Name;
            if (name.Contains("Redis", StringComparison.OrdinalIgnoreCase)) return true;
            if (ex.InnerException != null && IsRedisFailure(ex.InnerException)) return true;
            var msg = ex.Message ?? string.Empty;
            return msg.Contains("Redis", StringComparison.OrdinalIgnoreCase)
                   || msg.Contains("Connection refused", StringComparison.OrdinalIgnoreCase)
                   || msg.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class RedisUnavailableException : Exception
        {
            public RedisUnavailableException(string message) : base(message) { }
        }
    }
}
