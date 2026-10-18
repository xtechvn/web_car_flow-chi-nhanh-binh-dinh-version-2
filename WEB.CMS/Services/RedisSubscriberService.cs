using Entities.ViewModels.Car;
using Microsoft.AspNetCore.SignalR;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.CMS.Services
{
    public class RedisSubscriberService : BackgroundService
    {
        private readonly RedisConn _redisService;
        private readonly IHubContext<CarHub> _hubContext;
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IConfiguration _configuration;

        public RedisSubscriberService(
            RedisConn redisService,
            IHubContext<CarHub> hubContext,
            IVehicleInspectionRepository vehicleInspectionRepository,
            IAllCodeRepository allCodeRepository,
            IConfiguration configuration)
        {
            _redisService = redisService;
            _hubContext = hubContext;
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _redisService.Connect();
            LogHelper.InsertLogTelegram("RedisSubscriberService - ExecuteAsync: đã kết nối");

            // CartoFactory (xe đến nhà máy): chỉ nhận xe đăng ký TRƯỚC cut-off hôm nay.
            // Nếu publisher cũ vẫn đẩy nhầm sau cut-off → chuyển sang kênh RegisteredVehicle.
            await _redisService.SubscribeAsync("Add_ReceiveRegistration_BinhDinh", async (RegistrationRecord record) =>
            {
                await EnrichRecordAsync(record);
                if (await IsAfterTodayCutoffAsync(record.RegistrationTime))
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveRegistration_DK", record);
                    return;
                }
                await _hubContext.Clients.All.SendAsync("ReceiveRegistration", record);
            });

            await _redisService.Subscribed_Cam_Async("ListCartoFactory_Cam", async (CartoFactoryModel detail) =>
            {
                await _hubContext.Clients.All.SendAsync("ListCartoFactory_Da_SL", detail);
            });

            // RegisteredVehicle (xe đăng ký online sau cut-off)
            await _redisService.SubscribeAsync("Add_ReceiveRegistration_BinhDinh_DK", async (RegistrationRecord record) =>
            {
                await EnrichRecordAsync(record);
                await _hubContext.Clients.All.SendAsync("ReceiveRegistration_DK", record);
            });

            await Task.CompletedTask;
        }

        private async Task EnrichRecordAsync(RegistrationRecord record)
        {
            var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(record.Id);
            record.CreateTime = record.RegistrationTime.ToString("HH:mm dd/MM/yyyy");
            record.TrangThai = detail != null ? detail.TrangThai : 1;
        }

        private async Task<bool> IsAfterTodayCutoffAsync(DateTime registrationTime)
        {
            var hours = 17;
            var minutes = 55;
            try
            {
                var timeReset = await _allCodeRepository.GetListSortByName(AllCodeType.TIME_RESET);
                if (timeReset != null && timeReset.Count > 0 && timeReset[0].UpdateTime.HasValue)
                {
                    hours = timeReset[0].UpdateTime.Value.Hour;
                    minutes = timeReset[0].UpdateTime.Value.Minute;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RedisSubscriberService - đọc TIME_RESET lỗi: " + ex.Message);
            }

            var now = DateTime.Now;
            var cutoff = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
            return registrationTime >= cutoff;
        }
    }
}
