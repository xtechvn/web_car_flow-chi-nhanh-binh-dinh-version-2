namespace XTECH_FRONTEND.Services.Registration
{
    /// <summary>
    /// Cấu hình section "Registration" trong appsettings.json.
    /// </summary>
    public class RegistrationOptions
    {
        public const string SectionName = "Registration";

        /// <summary>Mã site, dùng làm tiền tố cho mọi key Redis. VD: BINH_DUONG.</summary>
        public string Site { get; set; } = "BINH_DUONG";

        /// <summary>LocationType gửi kèm message để consumer phân biệt nhà máy.</summary>
        public int LocationType { get; set; } = 3;

        /// <summary>Số phút khóa không cho cùng 1 biển số đăng ký lại.</summary>
        public int LockMinutes { get; set; } = 15;

        /// <summary>Số thứ tự reset về 1 trước giờ TIME_RESET bao nhiêu phút (quy tắc gốc: 5).</summary>
        public int CounterResetOffsetMinutes { get; set; } = 5;

        /// <summary>Giờ reset mặc định (HH:mm) khi không đọc được AllCode.TIME_RESET.</summary>
        public string DefaultResetTime { get; set; } = "17:55";

        /// <summary>Số giây cache giờ reset đọc từ SQL.</summary>
        public int ResetTimeCacheSeconds { get; set; } = 60;

        /// <summary>Số request tối đa mỗi IP trong 1 cửa sổ thời gian.</summary>
        public int RateLimitPerWindow { get; set; } = 20;

        /// <summary>Độ dài cửa sổ rate limit (giây).</summary>
        public int RateLimitWindowSeconds { get; set; } = 60;

        /// <summary>Chu kỳ (giây) job nền đẩy lại các message publish lỗi.</summary>
        public int PendingRepublishSeconds { get; set; } = 10;

        public int RedisDb { get; set; } = 15;
    }
}
