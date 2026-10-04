using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.IRepositories
{
    public interface IGoogleSheetsService
    {
        Task<int> GetDailyQueueCountAsync();
        Task<bool> SaveRegistrationAsync(RegistrationRecord record);
        Task<DateTime?> GetLastSubmissionTimeAsync(string phoneNumber);
        Task UpdateLastSubmissionTimeAsync(string phoneNumber, DateTime submissionTime);
        Task<int> GetDailyQueueCountRedis(DateTime? time);
    }
}
