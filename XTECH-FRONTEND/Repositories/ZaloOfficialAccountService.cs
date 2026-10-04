using System.Text.Json.Serialization;
using System.Text;
using System.Text.Json;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Repositories
{
    public class ZaloOfficialAccountService : IZaloService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ZaloOfficialAccountService> _logger;

        // Zalo credentials - Updated with your tokens
        private const string ACCESS_TOKEN = "xOciHlhYNd_kYwz0-UDOS-s8gqZw-3iDhVB6Pk61HYkakUbYyTzc3PwpaZUGgmvpWisXJkxCCto-zROnykq_IRY4eZJJw59RakBLDSRgQLwX-ieCqTGwLeRhjX3q_K9raFNX88Ba44t8YvCGkhy8Riwvcm2-jsuotDxxMQwpRMFZtSC0bFH2Iuw7iG-QkmHq_fsiCx-7ENsgewn6kxbTUEhGyr-QkcCQyfpeMgARNoAmhSGPtA8vTRQKp3FviZbOj9oD1DIw2aYUfPjprAef5vcNec-Elou7v8UxVlBgFK7bcRDolwfEJF6dt0pYc69hyRAVBUQf7LMclP0orOe1Ohgszb7MgNWNmS-BS9VJDotUnRHmZCGc5EhterAEqICf-j2cLB3uBnV1tzDxikiaFlpZhraZNiGA-lbQTG";
        private const string REFRESH_TOKEN = "xnzWFL2bVapi7MCxKwHuGEyUPpDGloG5j4HyVtwg7m_3GNuS0PSoP_DyMrnDtWjeeMT6Oc7O2KIe84nyHEDLIlWzBcGcrMaVnovs3ZxZ9plx3Mu-0SaWIE9q9MmQlpyRfdTR7sEl0X-H4MmaAgqmNkzzQMqpdJHeqs14UXRSFM7s94LXIv8KH9XE3K9KXKvGg4KDIas8IcUsGmb0NgPnMeyV2Y4oereKZ5b6CMsy35wqP5P5RxCMDvDsMICrjZeGsa510pw_6IlFL70y2ve1BFDyNLbCjHHIXdjPHblPDbwuBoXEVjDNDPSDF35Gx3aoaHbwGacX1KoOR5nYIQisLAPxK6zAkH1u-7b_J1N5EtlkV3vM2wfCJgrM4bqLhdXlv75fUYEh97RC5sq3PZYBZoX2KBvwHW";

        // Zalo API endpoints - Updated to v3.0
        private const string ZALO_API_BASE = "https://openapi.zalo.me/v3.0/oa";
        private const string USER_DETAIL_ENDPOINT = "/user/detail";
        private const string SEND_MESSAGE_ENDPOINT = "/message/cs";

        private readonly JsonSerializerOptions _jsonOptions;

        public ZaloOfficialAccountService(HttpClient httpClient, ILogger<ZaloOfficialAccountService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };

            // Configure HttpClient with access token
            _httpClient.DefaultRequestHeaders.Add("access_token", ACCESS_TOKEN);
        }

        public async Task<(bool Success, string Status)> SendRegistrationNotificationAsync(RegistrationRecord record)
        {
            try
            {
                _logger.LogInformation($"Starting Zalo notification process for {record.PhoneNumber}");

                // Step 1: Get user detail by phone number
                var userDetail = await GetUserDetailByPhoneAsync(record.PhoneNumber);
                if (userDetail == null)
                {
                    var status = "Số điện thoại này chưa được Approve Zalo OA";
                    _logger.LogWarning($"User not found for phone {record.PhoneNumber}");
                    return (false, status);
                }

                // Step 2: Check if user is follower
                if (!userDetail.user_is_follower)
                {
                    var status = "Số điện thoại này chưa được Approve Zalo OA";
                    _logger.LogWarning($"User {record.PhoneNumber} is not following the OA");
                    return (false, status);
                }

                // Step 3: Generate message content
                var message = GenerateRegistrationMessage(record, userDetail);

                // Step 4: Send message
                var (sendSuccess, sendMessage) = await SendMessageToUserAsync(userDetail.user_id, message);

                if (sendSuccess)
                {
                    var status = "Gửi dữ liệu thành công tới zalo cá nhân";
                    _logger.LogInformation($"Zalo message sent successfully to {record.PhoneNumber}");
                    return (true, status);
                }
                else
                {
                    var status = $"Lỗi gửi tin nhắn Zalo: {sendMessage}";
                    _logger.LogError($"Failed to send Zalo message to {record.PhoneNumber}: {sendMessage}");
                    return (false, status);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendRegistrationNotificationAsync - ZaloOfficialAccountService. " + ex);

                var status = $"Lỗi hệ thống Zalo: {ex.Message}";
                _logger.LogError(ex, $"Error in Zalo notification process for {record.PhoneNumber}");
                return (false, status);
            }
        }

        public async Task<ZaloUserData?> GetUserDetailByPhoneAsync(string phoneNumber)
        {
            try
            {
                var url = $"{ZALO_API_BASE}{USER_DETAIL_ENDPOINT}";
                var requestData = new { phone = phoneNumber };
                var queryString = $"?data={Uri.EscapeDataString(JsonSerializer.Serialize(requestData))}";

                _logger.LogInformation($"Getting user detail for phone: {phoneNumber}");

                var response = await _httpClient.GetAsync(url + queryString);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug($"Zalo user detail response: {content}");

                    var result = JsonSerializer.Deserialize<ZaloUserDetailResponse>(content, _jsonOptions);

                    if (result?.Error == 0 && result.Data != null)
                    {
                        _logger.LogInformation($"Found user: {result.Data.display_name} (ID: {result.Data.user_id})");
                        return result.Data;
                    }
                    else
                    {
                        _logger.LogWarning($"Zalo API error: {result?.Error} - {result?.Message}");
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"HTTP error {response.StatusCode}: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserDetailByPhoneAsync - ZaloOfficialAccountService. " + ex);
                _logger.LogError(ex, $"Exception getting user detail for phone {phoneNumber}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> SendMessageToUserAsync(string userId, string message)
        {
            try
            {
                var url = $"{ZALO_API_BASE}{SEND_MESSAGE_ENDPOINT}";

                var requestPayload = new ZaloMessageRequest
                {
                    Recipient = new ZaloRecipient { user_id = userId },
                    Message = new ZaloMessage { Text = message }
                };

                var json = JsonSerializer.Serialize(requestPayload, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Sending message to user {userId}");
                _logger.LogDebug($"Message payload: {json}");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug($"Zalo send message response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ZaloMessageResponse>(responseContent, _jsonOptions);

                    if (result?.Error == 0)
                    {
                        _logger.LogInformation($"Message sent successfully to user {userId}");
                        return (true, "Message sent successfully");
                    }
                    else
                    {
                        var errorMsg = $"Zalo API error: {result?.Error} - {result?.Message}";
                        _logger.LogError(errorMsg);
                        return (false, errorMsg);
                    }
                }
                else
                {
                    var errorMsg = $"HTTP error {response.StatusCode}: {responseContent}";
                    _logger.LogError(errorMsg);
                    return (false, errorMsg);
                }
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("SendMessageToUserAsync - ZaloOfficialAccountService. " + ex);
                var errorMsg = $"Exception sending message: {ex.Message}";
                _logger.LogError(ex, errorMsg);
                return (false, errorMsg);
            }
        }

        private string GenerateRegistrationMessage(RegistrationRecord record, ZaloUserData userDetail)
        {
            var message = $"🎉 XIN CHÀO {userDetail.display_name.ToUpper()}!\n\n" +
                $"✅ ĐĂNG KÝ XE THÀNH CÔNG!\n\n" +
                $" Tên khách hàng: {record.Name}\n" +
                $"📱 Số điện thoại: {record.PhoneNumber}\n" +
                $"🚗 Biển số xe: {record.PlateNumber}\n" +
                $"🚗 Trọng tải xe: {record.Referee}\n" +
                $"🎫 Số GPLX(3 số cuối giấy phép lái xe): {record.GPLX}\n" +
                $"🎫 Số thứ tự của bạn: {record.QueueNumber:D3}\n" +
                $"⏰ Thời gian đăng ký: {record.RegistrationTime:dd/MM/yyyy HH:mm}\n\n" +
                $"📍 VUI LÒNG:\n" +
                $"• Chuẩn bị đầy đủ giấy tờ xe\n" +
                $"• Có mặt đúng giờ theo thứ tự\n" +
                $"• Theo dõi cập nhật qua Zalo\n\n" +
                $"💡 Ước tính thời gian chờ: {record.QueueNumber * 5} phút\n" +
                $"🔔 Chúng tôi sẽ thông báo khi đến lượt bạn!\n\n" +
                $"📞 Hotline hỗ trợ: 1900-1234\n" +
                $"🌐 Website: https://x-tech.vn\n\n" +
                $"Cảm ơn bạn đã sử dụng dịch vụ! 🙏";
            return message;
        }
    }
}
