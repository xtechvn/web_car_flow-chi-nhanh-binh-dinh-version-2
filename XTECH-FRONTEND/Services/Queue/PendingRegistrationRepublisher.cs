using Microsoft.Extensions.Options;
using XTECH_FRONTEND.Services.Registration;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.Queue
{
    /// <summary>
    /// Job nền: định kỳ lấy message trong outbox Redis và publish lại vào RabbitMQ.
    /// </summary>
    public class PendingRegistrationRepublisher : BackgroundService
    {
        private const int MaxPerRound = 200;

        private readonly PendingRegistrationStore _store;
        private readonly IRegistrationQueuePublisher _publisher;
        private readonly RegistrationOptions _options;
        private readonly ILogger<PendingRegistrationRepublisher> _logger;

        public PendingRegistrationRepublisher(
            PendingRegistrationStore store,
            IRegistrationQueuePublisher publisher,
            IOptions<RegistrationOptions> options,
            ILogger<PendingRegistrationRepublisher> logger)
        {
            _store = store;
            _publisher = publisher;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromSeconds(Math.Max(1, _options.PendingRepublishSeconds));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RepublishRoundAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PendingRegistrationRepublisher lỗi");
                    _ = Task.Run(() => LogHelper.InsertLogTelegram("PendingRegistrationRepublisher lỗi: " + ex.Message));
                }

                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task RepublishRoundAsync(CancellationToken stoppingToken)
        {
            for (var i = 0; i < MaxPerRound && !stoppingToken.IsCancellationRequested; i++)
            {
                var message = await _store.PopOldestAsync();
                if (message == null)
                {
                    return;
                }

                if (!_publisher.Publish(message))
                {
                    await _store.ReturnOldestAsync(message);
                    return;
                }

                _logger.LogInformation("Đã publish lại message {MessageId} - số {QueueNumber}", message.MessageId, message.QueueNumber);
            }
        }
    }
}
