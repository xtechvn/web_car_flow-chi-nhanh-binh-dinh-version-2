using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.IRepositories
{
    public interface IZaloService
    {
        Task<(bool Success, string Status)> SendRegistrationNotificationAsync(RegistrationRecord record);
        Task<ZaloUserData?> GetUserDetailByPhoneAsync(string phoneNumber);
        Task<(bool Success, string Message)> SendMessageToUserAsync(string userId, string message);
    }
}
