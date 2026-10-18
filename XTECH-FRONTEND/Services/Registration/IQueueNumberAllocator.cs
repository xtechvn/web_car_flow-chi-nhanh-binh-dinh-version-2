namespace XTECH_FRONTEND.Services.Registration
{
    public enum AllocationStatus
    {
        /// <summary>Cấp số mới thành công.</summary>
        Allocated,

        /// <summary>Biển số đã đăng ký trong thời gian khóa, trả lại số cũ.</summary>
        AlreadyRegistered,

        /// <summary>IP gửi quá nhiều request trong cửa sổ rate limit.</summary>
        RateLimited
    }

    public class AllocationResult
    {
        public AllocationStatus Status { get; set; }
        public int QueueNumber { get; set; }

        /// <summary>Số giây còn lại của khóa biển số hoặc của cửa sổ rate limit.</summary>
        public int RemainingSeconds { get; set; }
    }

    public interface IQueueNumberAllocator
    {
        Task<AllocationResult> AllocateAsync(string plateNumber, RegistrationWindow window, string clientIp);
    }
}
