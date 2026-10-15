using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.Queue
{
    /// <summary>
    /// Publisher RabbitMQ dùng chung 1 connection + 1 channel cho toàn app (Singleton).
    /// IModel của RabbitMQ.Client không thread-safe nên mọi lần publish đi qua lock.
    /// Queue khai báo durable, message persistent, có publisher confirm để không mất dữ liệu khi broker restart.
    /// </summary>
    public sealed class RabbitMqRegistrationPublisher : IRegistrationQueuePublisher, IDisposable
    {
        private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(5);

        private readonly ConnectionFactory _factory;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqRegistrationPublisher> _logger;
        private readonly object _sync = new();

        private IConnection? _connection;
        private IModel? _channel;

        public RabbitMqRegistrationPublisher(IConfiguration configuration, ILogger<RabbitMqRegistrationPublisher> logger)
        {
            _logger = logger;
            _queueName = configuration["Queue:QueueRegistration"] ?? "QUEUE_CAR_REGISTRATION_BINH_DUONG";

            // "Queue:Port" (15672) là cổng web quản trị, AMQP dùng "Queue:AmqpPort" (mặc định 5672)
            var amqpPort = int.TryParse(configuration["Queue:AmqpPort"], out var port) ? port : AmqpTcpEndpoint.UseDefaultPort;

            _factory = new ConnectionFactory
            {
                HostName = configuration["Queue:Host"],
                VirtualHost = configuration["Queue:V_Host"],
                UserName = configuration["Queue:Username"],
                Password = configuration["Queue:Password"],
                Port = amqpPort,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };
        }

        public bool Publish(RegistrationQueueMessage message)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            lock (_sync)
            {
                try
                {
                    var channel = EnsureChannel();

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
                    properties.MessageId = message.MessageId.ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                    channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: properties, body: body);

                    if (channel.WaitForConfirms(ConfirmTimeout))
                    {
                        return true;
                    }

                    _logger.LogWarning("RabbitMQ không xác nhận message {MessageId}", message.MessageId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Publish RabbitMQ lỗi cho message {MessageId}", message.MessageId);
                    _ = Task.Run(() => LogHelper.InsertLogTelegram("RabbitMqRegistrationPublisher - publish lỗi: " + ex.Message));
                    ResetChannel();
                }

                return false;
            }
        }

        private IModel EnsureChannel()
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            ResetChannel();
            _connection = _factory.CreateConnection("XTECH-FRONTEND-registration");
            _channel = _connection.CreateModel();
            _channel.ConfirmSelect();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            return _channel;
        }

        private void ResetChannel()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            _channel = null;
            _connection = null;
        }

        public void Dispose()
        {
            lock (_sync)
            {
                ResetChannel();
            }
        }
    }
}
