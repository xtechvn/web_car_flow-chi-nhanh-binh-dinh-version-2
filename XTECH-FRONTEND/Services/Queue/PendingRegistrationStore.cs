using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Services.Registration;

namespace XTECH_FRONTEND.Services.Queue
{
    /// <summary>
    /// Outbox trên Redis: lưu các message đã cấp số nhưng publish RabbitMQ thất bại,
    /// để <see cref="PendingRegistrationRepublisher"/> đẩy lại sau.
    /// </summary>
    public class PendingRegistrationStore
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RegistrationOptions _options;

        public PendingRegistrationStore(IConnectionMultiplexer redis, IOptions<RegistrationOptions> options)
        {
            _redis = redis;
            _options = options.Value;
        }

        private RedisKey Key => $"pending:registration:{_options.Site}";

        private IDatabase Db => _redis.GetDatabase(_options.RedisDb);

        public Task PushAsync(RegistrationQueueMessage message)
        {
            return Db.ListLeftPushAsync(Key, JsonConvert.SerializeObject(message));
        }

        /// <summary>Lấy message cũ nhất ra khỏi outbox, null nếu rỗng.</summary>
        public async Task<RegistrationQueueMessage?> PopOldestAsync()
        {
            var value = await Db.ListRightPopAsync(Key);
            return value.IsNullOrEmpty ? null : JsonConvert.DeserializeObject<RegistrationQueueMessage>(value!);
        }

        /// <summary>Trả message về lại vị trí cũ nhất khi publish lại vẫn lỗi.</summary>
        public Task ReturnOldestAsync(RegistrationQueueMessage message)
        {
            return Db.ListRightPushAsync(Key, JsonConvert.SerializeObject(message));
        }
    }
}
