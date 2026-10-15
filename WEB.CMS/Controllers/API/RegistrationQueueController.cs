using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Cryptography;
using System.Text;
using Utilities;
using WEB.CMS.Models;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers.API
{
    /// <summary>
    /// Nhận message đăng ký xe từ n8n (n8n đọc RabbitMQ rồi gọi vào đây) và ghi bảng VehicleInspection.
    /// Xác thực bằng header X-Api-Key, idempotent theo MessageId.
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("api/registration-queue")]
    public class RegistrationQueueController : ControllerBase
    {
        private const string ApiKeyHeader = "X-Api-Key";
        private const string ProcessingMarker = "processing";
        private static readonly TimeSpan ProcessingTimeout = TimeSpan.FromMinutes(2);

        private readonly RegistrationImportService _importService;
        private readonly RedisConn _redisService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationQueueController> _logger;

        public RegistrationQueueController(
            RegistrationImportService importService,
            RedisConn redisService,
            IConfiguration configuration,
            ILogger<RegistrationQueueController> logger)
        {
            _importService = importService;
            _redisService = redisService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 200: đã ghi (hoặc đã ghi trước đó) → n8n ack.
        /// 400: dữ liệu sai, gửi lại cũng vô ích → n8n ack + lưu lỗi.
        /// 401: sai API key. 409/500/503: lỗi tạm thời → n8n thử lại sau.
        /// </summary>
        [HttpPost("receive")]
        public async Task<IActionResult> Receive([FromBody] RegistrationQueueMessage message)
        {
            if (!IsApiKeyValid())
            {
                return Unauthorized(new { success = false, code = "UNAUTHORIZED", message = "Sai hoặc thiếu X-Api-Key" });
            }

            var errors = Validate(message);
            if (errors.Count > 0)
            {
                return BadRequest(new { success = false, code = "INVALID_MESSAGE", message = string.Join("; ", errors) });
            }

            if (!_redisService.EnsureConnected())
            {
                return StatusCode(503, new { success = false, code = "REDIS_UNAVAILABLE", message = "Không kết nối được Redis, thử lại sau" });
            }

            int redisDb = Convert.ToInt32(_configuration["Redis:Database:db_common"]);
            string processedKey = $"cms:registration:msg:{message.MessageId:N}";

            try
            {
                bool acquired = await _redisService.SetIfNotExistsAsync(processedKey, ProcessingMarker, ProcessingTimeout, redisDb);
                if (!acquired)
                {
                    var existing = await _redisService.GetAsync(processedKey, redisDb);
                    if (int.TryParse(existing, out var existingId) && existingId > 0)
                    {
                        return Ok(new { success = true, code = "DUPLICATE", message = "Message đã được xử lý trước đó", id = existingId, messageId = message.MessageId });
                    }
                    return StatusCode(409, new { success = false, code = "IN_PROGRESS", message = "Message đang được xử lý, thử lại sau", messageId = message.MessageId });
                }

                var record = new RegistrationRecord
                {
                    PlateNumber = message.PlateNumber.Trim().ToUpper(),
                    PhoneNumber = message.PhoneNumber?.Trim() ?? string.Empty,
                    Name = message.Name?.Trim() ?? string.Empty,
                    GPLX = message.GPLX?.Trim() ?? string.Empty,
                    Referee = message.Referee?.Trim() ?? string.Empty,
                    Camp = message.Camp?.Trim() ?? string.Empty,
                    QueueNumber = message.QueueNumber,
                    RegistrationTime = message.RegistrationTime,
                    ZaloStatus = message.ZaloStatus ?? string.Empty
                };
                var result = await _importService.ImportIfNotExistsAsync(record);
                if (!result.Success)
                {
                    await _redisService.DeleteAsync(processedKey, redisDb);
                    LogHelper.InsertLogTelegram($"RegistrationQueue - Insert SQL lỗi: {record.PlateNumber} - số {record.QueueNumber} - MessageId {message.MessageId}");
                    return StatusCode(500, new { success = false, code = "SQL_INSERT_FAILED", message = "Ghi SQL thất bại, thử lại sau", messageId = message.MessageId });
                }

                int keepHours = int.TryParse(_configuration["RegistrationConsumer:ProcessedKeyHours"], out var h) && h > 0 ? h : 72;
                await _redisService.SetAsync(processedKey, result.Id.ToString(), TimeSpan.FromHours(keepHours), redisDb);

                if (result.AlreadyExists)
                {
                    return Ok(new { success = true, code = "DUPLICATE", message = "Xe đã có trong hệ thống (đã được đẩy lại từ MongoDB)", id = result.Id, messageId = message.MessageId });
                }
                return Ok(new { success = true, code = "INSERTED", message = "Thêm mới thành công", id = result.Id, messageId = message.MessageId });
            }
            catch (Exception ex)
            {
                try { await _redisService.DeleteAsync(processedKey, redisDb); } catch { }
                LogHelper.InsertLogTelegram("Receive - RegistrationQueueController: " + ex);
                _logger.LogError(ex, "Lỗi xử lý message đăng ký {MessageId}", message.MessageId);
                return StatusCode(500, new { success = false, code = "SERVER_ERROR", message = "Lỗi hệ thống, thử lại sau", messageId = message.MessageId });
            }
        }

        private bool IsApiKeyValid()
        {
            string expected = _configuration["RegistrationConsumer:ApiKey"];
            if (string.IsNullOrWhiteSpace(expected)) return false;
            if (!Request.Headers.TryGetValue(ApiKeyHeader, out var provided)) return false;

            var a = Encoding.UTF8.GetBytes(expected);
            var b = Encoding.UTF8.GetBytes(provided.ToString());
            return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
        }

        private List<string> Validate(RegistrationQueueMessage message)
        {
            var errors = new List<string>();
            if (message == null)
            {
                errors.Add("Body rỗng");
                return errors;
            }
            if (message.MessageId == Guid.Empty) errors.Add("Thiếu MessageId");
            if (string.IsNullOrWhiteSpace(message.PlateNumber)) errors.Add("Thiếu PlateNumber");
            else if (message.PlateNumber.Length > 20) errors.Add("PlateNumber quá dài");
            if (message.QueueNumber <= 0) errors.Add("QueueNumber phải > 0");
            if (message.RegistrationTime == default) errors.Add("Thiếu RegistrationTime");

            string site = _configuration["RegistrationConsumer:Site"];
            if (!string.IsNullOrEmpty(site) && !string.Equals(message.Site, site, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Site '{message.Site}' không thuộc CMS này ({site})");
            }
            return errors;
        }
    }
}
