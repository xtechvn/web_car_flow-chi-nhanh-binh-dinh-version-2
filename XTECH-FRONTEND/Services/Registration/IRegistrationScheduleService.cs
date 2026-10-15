namespace XTECH_FRONTEND.Services.Registration
{
    public class RegistrationWindow
    {
        /// <summary>Ngày làm việc mà đăng ký hiện tại thuộc về (dùng làm key counter) = ngày của PeriodEnd.</summary>
        public DateTime BusinessDate { get; set; }

        /// <summary>Lần reset số thứ tự trước = PeriodEnd - 1 ngày.</summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>Lần reset số thứ tự kế tiếp = TIME_RESET - CounterResetOffsetMinutes.</summary>
        public DateTime PeriodEnd { get; set; }
    }

    public interface IRegistrationScheduleService
    {
        /// <summary>Giờ reset hôm nay (ngày hôm nay + giờ:phút của AllCode.TIME_RESET).</summary>
        Task<DateTime> GetTodayResetTimeAsync();

        Task<RegistrationWindow> GetCurrentWindowAsync(DateTime now);
    }
}
