namespace XTECH_FRONTEND.Model
{
    /// <summary>
    /// Message đẩy vào RabbitMQ khi tài xế đăng ký. Consumer dùng message này để insert bảng VehicleInspection.
    /// MessageId là khóa idempotent: consumer phải bỏ qua message đã xử lý.
    /// </summary>
    public class RegistrationQueueMessage
    {
        public Guid MessageId { get; set; }
        public string Site { get; set; } = string.Empty;
        public int LocationType { get; set; }

        /// <summary>Ngày làm việc (yyyy-MM-dd) mà số thứ tự thuộc về.</summary>
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
