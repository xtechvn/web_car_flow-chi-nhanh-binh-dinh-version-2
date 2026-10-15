using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace XTECH_FRONTEND.Services.Registration
{
    /// <summary>
    /// Cấp số thứ tự bằng 1 script Lua chạy atomic trên Redis:
    /// rate limit theo IP → chống trùng biển số → INCR counter của ngày làm việc.
    /// Redis thực thi script tuần tự nên N request đồng thời luôn nhận N số khác nhau.
    /// </summary>
    public class RedisQueueNumberAllocator : IQueueNumberAllocator
    {
        // KEYS[1] = rate limit key, KEYS[2] = plate lock key, KEYS[3] = counter key
        // ARGV[1] = rate limit, ARGV[2] = rate window (s), ARGV[3] = lock ttl (s), ARGV[4] = counter ttl (s)
        // Trả về {status, queueNumber, remainingSeconds}: status 1 = cấp mới, 0 = đã đăng ký, -1 = rate limited
        private const string AllocateScript = @"
local hits = redis.call('INCR', KEYS[1])
if hits == 1 then redis.call('EXPIRE', KEYS[1], ARGV[2]) end
if hits > tonumber(ARGV[1]) then
  return {-1, 0, redis.call('TTL', KEYS[1])}
end
local existing = redis.call('GET', KEYS[2])
if existing then
  return {0, tonumber(existing), redis.call('TTL', KEYS[2])}
end
local n = redis.call('INCR', KEYS[3])
if n == 1 then redis.call('EXPIRE', KEYS[3], ARGV[4]) end
redis.call('SET', KEYS[2], n, 'EX', ARGV[3])
return {1, n, tonumber(ARGV[3])}";

        private const int CounterTtlSeconds = 3 * 24 * 60 * 60;

        private readonly IConnectionMultiplexer _redis;
        private readonly RegistrationOptions _options;

        public RedisQueueNumberAllocator(IConnectionMultiplexer redis, IOptions<RegistrationOptions> options)
        {
            _redis = redis;
            _options = options.Value;
        }

        public async Task<AllocationResult> AllocateAsync(string plateNumber, DateTime businessDate, string clientIp)
        {
            var site = _options.Site;
            var day = businessDate.ToString("yyyyMMdd");
            var plateKey = NormalizePlate(plateNumber);

            var keys = new RedisKey[]
            {
                $"ratelimit:{site}:{clientIp}",
                $"lock:{site}:{plateKey}:{day}",
                $"counter:{site}:{day}"
            };
            var args = new RedisValue[]
            {
                _options.RateLimitPerWindow,
                _options.RateLimitWindowSeconds,
                _options.LockMinutes * 60,
                CounterTtlSeconds
            };

            var db = _redis.GetDatabase(_options.RedisDb);
            var result = (RedisResult[])(await db.ScriptEvaluateAsync(AllocateScript, keys, args))!;

            var status = (int)result[0];
            return new AllocationResult
            {
                Status = status switch
                {
                    1 => AllocationStatus.Allocated,
                    0 => AllocationStatus.AlreadyRegistered,
                    _ => AllocationStatus.RateLimited
                },
                QueueNumber = (int)result[1],
                RemainingSeconds = Math.Max(0, (int)result[2])
            };
        }

        public static string NormalizePlate(string plateNumber)
        {
            return Regex.Replace(plateNumber.ToUpperInvariant(), @"[^0-9A-Z]", "");
        }
    }
}
