using DnsClient;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using Telegram.Bot.Types;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Repositories;
using XTECH_FRONTEND.Services;
using XTECH_FRONTEND.Services.BackgroundQueue;
using XTECH_FRONTEND.Services.RedisWorker;
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
        private readonly WorkQueueClient _workQueueClient;
        private readonly RedisConn redisService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<RegistrationHub> _hubContext;
        private readonly IInsertQueue _insertQueue;
        public CarRegistrationController(
            IValidationService validationService,
            IGoogleSheetsService googleSheetsService,
            IGoogleFormsService googleFormsService,
            IZaloService zaloService,
            ILogger<CarRegistrationController> logger,
            IConfiguration configuration,
            IMongoService mongoService, IHubContext<RegistrationHub> hubContext, IInsertQueue insertQueue)
        {
            _validationService = validationService;
            _googleSheetsService = googleSheetsService;
            _googleFormsService = googleFormsService;
            _zaloService = zaloService;
            _logger = logger;
            _workQueueClient = new WorkQueueClient(configuration);
            _mongoService = mongoService;
            redisService = new RedisConn(configuration);
            redisService.Connect();
            _configuration = configuration;
            _hubContext = hubContext;
            _insertQueue = insertQueue;
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
        [HttpPost("registerV2")]
        public async Task<ActionResult<CarRegistrationResponse>> RegisterCarV2([FromBody] CarRegistrationRequest request)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var now = DateTime.Now;
                var hours = now.Hour;
                var minutes = now.Minute;

                // Kiểm tra khoảng 17:55 đến 18:00

      
                string cache_name = "BinhDinh_PlateNumber_" + request.PlateNumber.Replace("-", "_")+DateTime.Now.ToString("dd_MM_yyyy");
                var data = await redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                if (data != null && data.Trim() != "")
                {

                    var data_detail = JsonConvert.DeserializeObject<RegistrationRecord>(data);
                    return BadRequest(new CarRegistrationResponse
                    {
                        Success = false,
                        Message = data_detail.PlateNumber + $" đã đăng ký số " + data_detail.QueueNumber + ", Vui lòng đợi 15 phút trước khi gửi lại",
                        RemainingTimeMinutes = 15
                    });
                }
                var queueNumber = await _googleSheetsService.GetDailyQueueCountRedis(DateUtil.StringToDateTime( request.Timedow));


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
                    Camp = request.Camp,
                };
                redisService.Set15P(cache_name, JsonConvert.SerializeObject(registrationRecord), Convert.ToInt32(_configuration["Redis:Database:db_common"]));

                var InsertMG = await _mongoService.Insert(registrationRecord);
                if (InsertMG == 0)
                {
                    InsertMG = await _mongoService.Insert(registrationRecord);
                }
               
               
                await _hubContext.Clients.All.SendAsync("ReceiveRegistration_FE", registrationRecord);
               
                stopwatch.Stop(); // Dừng đo thời gian
                
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    LogHelper.InsertLogTelegram("TG sử lý " + request.PlateNumber + ": " + stopwatch.ElapsedMilliseconds    );
                    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                    var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] SLOW: {stopwatch.ElapsedMilliseconds}ms - Plate: {request.PlateNumber}";                  
                    var logPath = Path.Combine(logDirectory, "slow_requests.log");
                    using (var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fs))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                    }
                }
                await _insertQueue.EnqueueAsync(new InsertJob
                {
                    Data = new CarRegistrationResponse
                    {
                        Camp = registrationRecord.Camp,
                        GPLX = registrationRecord.GPLX,
                        Name = registrationRecord.Name,
                        PlateNumber = registrationRecord.PlateNumber,
                        PhoneNumber = registrationRecord.PhoneNumber,
                        QueueNumber = registrationRecord.QueueNumber,
                        Referee = registrationRecord.Referee,
                        RegistrationTime = registrationRecord.RegistrationTime,
                        ZaloStatus = registrationRecord.ZaloStatus,
                        LocationType=3,
                    }
                });
                // Return success response
                return Ok(new CarRegistrationResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công!",
                    QueueNumber = queueNumber,
                    RegistrationTime = registrationRecord.RegistrationTime,
                    PlateNumber = registrationRecord.PlateNumber,
                    PhoneNumber = registrationRecord.PhoneNumber,
                    ZaloStatus = "Đang xử lý...",

                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - RegisterCarV2: " + ex.Message);
                _logger.LogError(ex, "Error processing car registration");
                return StatusCode(500, new CarRegistrationResponse
                {
                    Success = false,
                    Message = "Lỗi hệ thống, vui lòng thử lại sau"
                });
            }
        }
        [HttpPost("insert")]
        public async Task<ActionResult<object>> Insert([FromBody] CarRegistrationResponse request)
        {
            try
            {
                var message = $"✅ ĐĂNG KÝ XE THÀNH CÔNG!\n\n" +
                $" Tên khách hàng: {request.Name}\n" +
                $"📱 Số điện thoại: {request.PhoneNumber}\n" +
                $"🚗 Biển số xe: {request.PlateNumber}\n" +
                $"🚗 Trọng tải xe: {request.Referee}\n" +
                $"🎫 Hoàn hảo/Trại : {request.GPLX}\n" +
                $"🎫 Số thứ tự của bạn: {request.QueueNumber:D3}\n" +
                $"⏰ Thời gian đăng ký: {request.RegistrationTime:dd/MM/yyyy HH:mm}\n\n" +
                $"📍 VUI LÒNG:\n" +
                $"• Chuẩn bị đầy đủ giấy tờ xe\n" +
                $"• Có mặt đúng giờ theo thứ tự\n" +
                $"• Theo dõi cập nhật qua Zalo\n\n" +
                $"🔔 Chúng tôi sẽ thông báo khi đến lượt bạn!\n\n" +
                $"📞 Hotline hỗ trợ: 1900-1234\n" +
                $"🌐 Website: https://cargilllongan.com\n\n" +
                $"Cảm ơn bạn đã sử dụng dịch vụ! ";
                string url = "http://api.longancargill.com/api/vehicleInspection/Insert";
                var client = new HttpClient();
                var request_api = new HttpRequestMessage(HttpMethod.Post, url);
                request_api.Content = new StringContent(JsonConvert.SerializeObject(request), null, "application/json");
                var response = await client.SendAsync(request_api);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    LogHelper.InsertLogTelegram("Insert - lỗi " );
                }
                LogHelper.InsertLogTelegram(message);
                return StatusCode(200, "thành công");
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CarRegistrationController - GetQueueStatus: " + ex.Message);
                _logger.LogError(ex, "Error getting queue status");
                return StatusCode(500, "Lỗi hệ thống");
            }
        }

    }
}
