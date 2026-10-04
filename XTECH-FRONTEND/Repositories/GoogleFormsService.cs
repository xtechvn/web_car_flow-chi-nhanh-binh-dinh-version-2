using System.Net.Http;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Repositories
{
    public class GoogleFormsService : IGoogleFormsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleFormsService> _logger;
        private readonly IConfiguration _configuration;

        public GoogleFormsService(HttpClient httpClient, ILogger<GoogleFormsService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SubmitToGoogleFormAsync(RegistrationRecord record)
        {
            try
            {
                // Google Forms submission can be implemented here if needed
                // For now, just log the submission
                _logger.LogInformation($"Google Form submission simulated for: {record.PhoneNumber}");
                await Task.Delay(100); // Simulate API call
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SubmitToGoogleFormAsync - GoogleFormsService. " + ex);
                _logger.LogError(ex, "Error submitting to Google Form");
                return false;
            }
        }
    }
}
