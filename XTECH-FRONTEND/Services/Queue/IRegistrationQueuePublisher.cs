using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.Services.Queue
{
    public interface IRegistrationQueuePublisher
    {
        /// <summary>
        /// Publish message (persistent) và chờ broker xác nhận. Trả về false nếu không publish được.
        /// </summary>
        bool Publish(RegistrationQueueMessage message);
    }
}
