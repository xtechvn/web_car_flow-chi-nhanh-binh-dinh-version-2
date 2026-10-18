using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Repositories;
using XTECH_FRONTEND.Services.Queue;
using XTECH_FRONTEND.Services.Registration;
using XTECH_FRONTEND.Utilities;


namespace XTECH_FRONTEND.Controllers.CarRegistration
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarRegistrationController : Controller
    {
        private readonly IValidationService _validationService;
        private readonly IGoogleSheetsService _googleSheetsService;
        private readonly IGoogleFormsService _googleFormsService;
        private readonly IZaloService _zaloService;
        private readonly ILogger<CarRegistrationController> _logger;
        private readonly IMongoService _mongoService;
        private readonly IHubContext<RegistrationHub> _hubContext;
        private readonly IRegistrationScheduleService _scheduleService;
        private readonly IQueueNumberAllocator _queueNumberAllocator;
        private readonly IRegistrationQueuePublisher _queuePublisher;
        private readonly PendingRegistrationStore _pendingStore;
        private readonly RegistrationOptions _registrationOptions;
        public CarRegistrationController(
            IValidationService validationService,
            IGoogleSheetsService googleSheetsService,
            IGoogleFormsService googleFormsService,
            IZaloService zaloService,
            ILogger<CarRegistrationController> logger,
            IMongoService mongoService,
            IHubContext<RegistrationHub> hubContext,
            IRegistrationScheduleService scheduleService,
            IQueueNumberAllocator queueNumberAllocator,
            IRegistrationQueuePublisher queuePublisher,
            PendingRegistrationStore pendingStore,
            IOptions<RegistrationOptions> registrationOptions)
        {
            _validationService = validationService;
            _googleSheetsService = googleSheetsService;
            _googleFormsService = googleFormsService;
            _zaloService = zaloService;
            _logger = logger;
            _mongoService = mongoService;
            _hubContext = hubContext;
            _scheduleService = scheduleService;
            _queueNumberAllocator = queueNumberAllocator;
            _queuePublisher = queuePublisher;
            _pendingStore = pendingStore;
            _registrationOptions = registrationOptions.Value;
        }
        [HttpPost("register-V1")]
        public async Task<ActionResult<CarRegistrationResponse>> RegisterCar([FromBody] CarRegistrationRequest request)
        {
            try
            {
                var now = DateTime.Now;
                var hours = now.Hour;
                var minutes = now.Minute;

                _logger.LogInformation($"Car registration request received: {request.PhoneNumber} - {request.PlateNumber}");

                // Step 1: Validate input data
                var validationResult = _validationService.ValidateCarRegistration(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new CarRegistrationResponse
                    {
                        Success = false,
                        Message = string.Join(", ", validationResult.Errors)
                    });
                }

                // Step 2: Check time restriction (15 minutes rule)
                var timeRestriction = _validationService.CheckTimeRestriction(request.PlateNumber);
                if (!timeRestriction.CanSubmit)
                {
                    return BadRequest(new CarRegistrationResponse
                    {
                        Success = false,
                        Message = $"Vui lòng đợi {timeRestriction.RemainingMinutes} phút trước khi gửi lại",
                        RemainingTimeMinutes = timeRestriction.RemainingMinutes
                    });
                }

                // Step 3: Get current daily queue count
                var dailyCount = await _googleSheetsService.GetDailyQueueCountAsync();
                var queueNumber = dailyCount + 1;

                // Step 4: Create registration record with initial Zalo status
                var registrationRecord = new RegistrationRecord
                {
                    PhoneNumber = request.PhoneNumber,
                    PlateNumber = request.PlateNumber.ToUpper(),
                    Name = request.Name.ToUpper(),
                    Referee = request.Referee.ToUpper(),
                    GPLX = request.GPLX.ToUpper(),
                    QueueNumber = queueNumber,
                    RegistrationTime = DateTime.Now,
                    ZaloStatus = "Đang xử lý...",
                    Camp = request.Camp
                };

                // Step 5: Submit to Google Form
                var formSubmissionSuccess = await _googleFormsService.SubmitToGoogleFormAsync(registrationRecord);
                if (!formSubmissionSuccess)
                {
                    _logger.LogWarning("Google Form submission failed, but continuing...");
                }

                // Step 6: Send Zalo notification and get status
                var (zaloSuccess, zaloStatus) = await _zaloService.SendRegistrationNotificationAsync(registrationRecord);

                // Update registration record with Zalo status
                registrationRecord.ZaloStatus = zaloStatus;

                // Step 7: Save to mogoDB
                await _mongoService.Insert(registrationRecord);
                // Step 7: Save to Google Sheets with Zalo status
                var sheetsSuccess = await _googleSheetsService.SaveRegistrationAsync(registrationRecord);
                if (!sheetsSuccess)
                {
                    return StatusCode(500, new CarRegistrationResponse
                    {
                        Success = false,
                        Message = "Lỗi hệ thống, vui lòng thử lại sau"
                    });
                }

                // Step 8: Update last submission time
                await _googleSheetsService.UpdateLastSubmissionTimeAsync(request.PlateNumber, DateTime.Now);

                // Return success response
                return Ok(new CarRegistrationResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công!",
                    QueueNumber = queueNumber,
                    RegistrationTime = registrationRecord.RegistrationTime,
                    PlateNumber = registrationRecord.PlateNumber,
                    PhoneNumber = registrationRecord.PhoneNumber,
                    ZaloStatus = zaloStatus,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - RegisterCar: " + ex.Message);
                _logger.LogError(ex, "Error processing car registration");
                return StatusCode(500, new CarRegistrationResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                });
            }
        }

        [HttpGet("check-restriction/{plateNumber}")]
        public ActionResult<TimeRestrictionResult> CheckTimeRestriction(string PlateNumber)
        {
            try
            {
                var result = _validationService.CheckTimeRestriction(PlateNumber);
                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - CheckTimeRestriction: " + ex.Message);
                _logger.LogError(ex, $"Error checking time restriction for {PlateNumber}");
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

        [HttpGet("queue-status")]
        public async Task<ActionResult<object>> GetQueueStatus()
        {
            try
            {
                var dailyCount = await _googleSheetsService.GetDailyQueueCountAsync();
                return Ok(new
                {
                    CurrentQueueNumber = dailyCount,
                    NextQueueNumber = dailyCount + 1,
                    Date = DateTime.Today.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - GetQueueStatus: " + ex.Message);
                _logger.LogError(ex, "Error getting queue status");
                return StatusCode(500, "Lỗi hệ thống");
            }
        }
        [HttpGet("check-zalo-user/{phoneNumber}")]
        public async Task<ActionResult> CheckZaloUser(string phoneNumber)
        {
            try
            {
                var userDetail = await _zaloService.GetUserDetailByPhoneAsync(phoneNumber);

                if (userDetail == null)
                {
                    return Ok(new
                    {
                        exists = false,
                        message = "Số điện thoại này chưa được Approve Zalo OA"
                    });
                }

                return Ok(new
                {
                    exists = true,
                    userId = userDetail.user_id,
                    displayName = userDetail.display_name,
                    isFollower = userDetail.user_is_follower,
                    lastInteraction = userDetail.user_last_interaction_date,
                    avatar = userDetail.Avatar,
                    status = userDetail.user_is_follower ? "Có thể gửi tin nhắn" : "Chưa follow OA"
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - CheckZaloUser: " + ex.Message);
                _logger.LogError(ex, $"Error checking Zalo user for {phoneNumber}");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Đăng ký xe: cấp số thứ tự ngay bằng Redis (atomic) rồi đẩy dữ liệu vào RabbitMQ.
        /// Không gọi API ngoài; consumer đọc queue để ghi SQL Server.
        /// </summary>
        [HttpPost("registerV2")]
        public async Task<ActionResult<CarRegistrationResponse>> RegisterCarV2([FromBody] CarRegistrationRequest request)
        {
            string plateForLog = request?.PlateNumber;
            string ipForLog = null;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var now = DateTime.Now;
                ipForLog = GetClientIp();

                if (request == null
                    || string.IsNullOrWhiteSpace(request.PlateNumber)
                    || string.IsNullOrWhiteSpace(request.PhoneNumber)
                    || string.IsNullOrWhiteSpace(request.Name)
                    || string.IsNullOrWhiteSpace(request.GPLX)
                    || string.IsNullOrWhiteSpace(request.Referee))
                {
                    return BadRequest(new CarRegistrationResponse
                    {
                        Success = false,
                        Message = "Vui lòng nhập đủ biển số, điện thoại, tên khách hàng, tên lái xe và tải trọng"
                    });
                }

                var window = await _scheduleService.GetCurrentWindowAsync(now);

                var plateNumber = request.PlateNumber.Trim().ToUpper();
                plateForLog = plateNumber;
                var allocation = await _queueNumberAllocator.AllocateAsync(plateNumber, window, ipForLog);
                if (allocation.Status == AllocationStatus.RateLimited)
                {
                    var waitMinutes = Math.Max(1, (int)Math.Ceiling(allocation.RemainingSeconds / 60.0));
                    return StatusCode(StatusCodes.Status429TooManyRequests, new CarRegistrationResponse
                    {
                        Success = false,
                        Message = $"Bạn gửi quá nhiều yêu cầu, vui lòng đợi {waitMinutes} phút rồi thử lại",
                        RemainingTimeMinutes = waitMinutes
                    });
                }

                if (allocation.Status == AllocationStatus.AlreadyRegistered)
                {
                    var waitMinutes = Math.Max(1, (int)Math.Ceiling(allocation.RemainingSeconds / 60.0));
                    return BadRequest(new CarRegistrationResponse
                    {
                        Success = false,
                        QueueNumber = allocation.QueueNumber,
                        Message = $"{plateNumber} đã đăng ký số {allocation.QueueNumber:D3}, vui lòng đợi {waitMinutes} phút trước khi gửi lại",
                        RemainingTimeMinutes = waitMinutes
                    });
                }

                var registrationRecord = new RegistrationRecord
                {
                    PhoneNumber = request.PhoneNumber.Trim(),
                    PlateNumber = plateNumber,
                    Name = request.Name.Trim().ToUpper(),
                    Referee = request.Referee.Trim().ToUpper(),
                    GPLX = request.GPLX.Trim().ToUpper(),
                    QueueNumber = allocation.QueueNumber,
                    RegistrationTime = now,
                    ZaloStatus = "Đang xử lý...",
                    Camp = request.Camp?.Trim() ?? string.Empty,
                };

                var message = new RegistrationQueueMessage
                {
                    MessageId = Guid.NewGuid(),
                    Site = _registrationOptions.Site,
                    LocationType = _registrationOptions.LocationType,
                    BusinessDate = window.BusinessDate.ToString("yyyy-MM-dd"),
                    QueueNumber = registrationRecord.QueueNumber,
                    RegistrationTime = registrationRecord.RegistrationTime,
                    PlateNumber = registrationRecord.PlateNumber,
                    PhoneNumber = registrationRecord.PhoneNumber,
                    Name = registrationRecord.Name,
                    GPLX = registrationRecord.GPLX,
                    Referee = registrationRecord.Referee,
                    Camp = registrationRecord.Camp,
                    ZaloStatus = registrationRecord.ZaloStatus
                };

                if (!_queuePublisher.Publish(message))
                {
                    try
                    {
                        await _pendingStore.PushAsync(message);
                        _logger.LogWarning("Publish lỗi, đã lưu outbox: {MessageId} - số {QueueNumber}", message.MessageId, message.QueueNumber);
                    }
                    catch (Exception outboxEx)
                    {
                        // Redis down + RabbitMQ fail: số đã cấp, vẫn trả thành công; cần recovery Mongo→SQL
                        _logger.LogError(outboxEx, "Publish lỗi và không lưu được outbox Redis: {MessageId} - số {QueueNumber}",
                            message.MessageId, message.QueueNumber);
                        _ = Task.Run(() => LogHelper.InsertLogTelegram(
                            $"registerV2 - mất outbox (Redis?): {plateNumber} số {message.QueueNumber} - {outboxEx.Message}"));
                    }
                }

                var insertMongo = await _mongoService.Insert(registrationRecord);
                if (insertMongo == 0)
                {
                    await _mongoService.Insert(registrationRecord);
                }

                await _hubContext.Clients.All.SendAsync("ReceiveRegistration_FE", registrationRecord);

                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    LogSlowRequest(plateNumber, stopwatch.ElapsedMilliseconds);
                }

                return Ok(new CarRegistrationResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công!",
                    QueueNumber = registrationRecord.QueueNumber,
                    RegistrationTime = registrationRecord.RegistrationTime,
                    PlateNumber = registrationRecord.PlateNumber,
                    PhoneNumber = registrationRecord.PhoneNumber,
                    ZaloStatus = registrationRecord.ZaloStatus,
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertRegistrationError("CarRegistrationController - RegisterCarV2", plateForLog, ipForLog, ex);
                _logger.LogError(ex, "Error processing car registration for {Plate}", plateForLog);
                return StatusCode(500, new CarRegistrationResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                });
            }
        }

        private string GetClientIp()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private void LogSlowRequest(string plateNumber, long elapsedMilliseconds)
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                Directory.CreateDirectory(logDirectory);
                var logPath = Path.Combine(logDirectory, "slow_requests.log");
                using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(fs);
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - SLOW: {elapsedMilliseconds}ms - Plate: {plateNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không ghi được slow_requests.log");
            }
            _ = Task.Run(() => LogHelper.InsertLogTelegram("TG xử lý " + plateNumber + ": " + elapsedMilliseconds + "ms"));
        }
    }
}
