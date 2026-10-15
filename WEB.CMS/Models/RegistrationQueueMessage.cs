namespace WEB.CMS.Models
{
    /// <summary>
    /// Message XTECH-FRONTEND đẩy vào RabbitMQ (queue QUEUE_CAR_REGISTRATION_BINH_DUONG).
    /// n8n đọc queue rồi POST nguyên body này vào /api/registration-queue/receive.
    /// Phải khớp với XTECH-FRONTEND/Model/RegistrationQueueMessage.cs.
    /// </summary>
    public class RegistrationQueueMessage
    {
        public Guid MessageId { get; set; }
        public string Site { get; set; } = string.Empty;
        public int LocationType { get; set; }
        public string BusinessDate { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GPLX { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string Camp { get; set; } = string.Empty;
        public string ZaloStatus { get; set; } = string.Empty;
    }
}
