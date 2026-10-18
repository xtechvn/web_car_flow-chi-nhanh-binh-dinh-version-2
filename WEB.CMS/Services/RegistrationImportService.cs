using Entities.ConfigModels;
using Entities.ViewModels.Car;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace WEB.CMS.Services
{
    public class ExistingRegistration
    {
        public int Id { get; set; }
        public int RecordNumber { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public DateTime RegisterDateOnline { get; set; }
    }

    public class ImportResult
    {
        public int Id { get; set; }
        public bool AlreadyExists { get; set; }
        public bool Success => Id > 0;
    }

    /// <summary>
    /// Ghi xe đăng ký online vào VehicleInspection. Dùng chung cho API n8n và chức năng đẩy lại từ MongoDB
    /// để hai đường cùng chống trùng theo một quy tắc.
    /// </summary>
    public class RegistrationImportService
    {
        private static readonly TimeSpan SameTimeTolerance = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan SamePlateTolerance = TimeSpan.FromHours(12);
        // Kiểm tra tồn tại + insert phải liền một khối, nếu không n8n và nút "đẩy lại" có thể cùng insert 1 xe
        private static readonly SemaphoreSlim ImportLock = new SemaphoreSlim(1, 1);

        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly RedisConn _redisService;
        private readonly WorkQueueClient _workQueueClient;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public RegistrationImportService(
            IVehicleInspectionRepository vehicleInspectionRepository,
            IAllCodeRepository allCodeRepository,
            RedisConn redisService,
            IConfiguration configuration,
            IOptions<DataBaseConfig> dataBaseConfig)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;
            _redisService = redisService;
            _configuration = configuration;
            _workQueueClient = new WorkQueueClient(configuration);
            _connectionString = dataBaseConfig.Value.SqlServer.ConnectionString;
        }

        public static string NormalizePlate(string? plate)
        {
            if (string.IsNullOrEmpty(plate)) return string.Empty;
            var sb = new StringBuilder(plate.Length);
            foreach (var ch in plate.ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Cùng số thứ tự và (cùng thời điểm đăng ký, hoặc cùng biển số trong vòng 12 giờ).
        /// Vế biển số để không đẩy trùng khi CMS đã sửa giờ; vế thời gian để không đẩy trùng khi CMS đã sửa biển số.
        /// </summary>
        public static bool IsSameRegistration(ExistingRegistration existing, int recordNumber, string plate, DateTime registrationTime)
        {
            if (existing.RecordNumber != recordNumber) return false;
            var diff = (existing.RegisterDateOnline - registrationTime).Duration();
            if (diff <= SameTimeTolerance) return true;
            return diff <= SamePlateTolerance && NormalizePlate(existing.VehicleNumber) == NormalizePlate(plate);
        }

        public async Task<List<ExistingRegistration>> GetExistingAsync(DateTime fromTime, DateTime toTime)
        {
            var list = new List<ExistingRegistration>();
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(
                @"SELECT Id, RecordNumber, VehicleNumber, RegisterDateOnline
                  FROM VehicleInspection
                  WHERE RegisterDateOnline >= @From AND RegisterDateOnline < @To", connection);
            command.Parameters.AddWithValue("@From", fromTime);
            command.Parameters.AddWithValue("@To", toTime);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (reader.IsDBNull(1) || reader.IsDBNull(3)) continue;
                list.Add(new ExistingRegistration
                {
                    Id = Convert.ToInt32(reader.GetValue(0)),
                    RecordNumber = Convert.ToInt32(reader.GetValue(1)),
                    VehicleNumber = reader.IsDBNull(2) ? string.Empty : reader.GetValue(2).ToString() ?? string.Empty,
                    RegisterDateOnline = reader.GetDateTime(3)
                });
            }
            return list;
        }

        public async Task<int?> FindExistingIdAsync(int recordNumber, string plate, DateTime registrationTime)
        {
            var existing = await GetExistingAsync(registrationTime - SamePlateTolerance, registrationTime + SamePlateTolerance);
            var match = existing.FirstOrDefault(e => IsSameRegistration(e, recordNumber, plate, registrationTime));
            return match?.Id;
        }

        /// <summary>Đã có trong SQL thì trả Id cũ (AlreadyExists), chưa có thì insert.</summary>
        public async Task<ImportResult> ImportIfNotExistsAsync(RegistrationRecord record)
        {
            await ImportLock.WaitAsync();
            try
            {
                var existingId = await FindExistingIdAsync(record.QueueNumber, record.PlateNumber, record.RegistrationTime);
                if (existingId.HasValue)
                {
                    return new ImportResult { Id = existingId.Value, AlreadyExists = true };
                }
                return new ImportResult { Id = await InsertAsync(record) };
            }
            finally
            {
                ImportLock.Release();
            }
        }

        /// <summary>Gọi sp_InsertVehicleInspection; thành công thì báo realtime cho CMS và đẩy queue audio. Trả về Id (≤ 0 là lỗi).</summary>
        private async Task<int> InsertAsync(RegistrationRecord record)
        {
            record.AudioPath = await _vehicleInspectionRepository.GetAudioPathByVehicleNumber(record.PlateNumber);

            int id = _vehicleInspectionRepository.SaveVehicleInspection(record);
            if (id <= 0) return id;

            record.Id = id;
            record.Bookingid = id;
            record.text_voice = "Mời biển số xe " + record.PlateNumber + " vào cân";
            await NotifyAfterInsertAsync(record);
            return id;
        }

        private async Task NotifyAfterInsertAsync(RegistrationRecord record)
        {
            try
            {
                // Rule: sau cut-off (TIME_RESET hôm nay) đến trước 00:00 → chỉ "Danh sách xe đăng ký" (RegisteredVehicle / kênh _DK).
                // Từ 00:00 đến cut-off → "Danh sách xe đến nhà máy" (CartoFactory / kênh chính). F5 đọc SQL cùng rule này.
                string channel = await ResolveRealtimeChannelAsync(record.RegistrationTime);
                if (_redisService.EnsureConnected())
                {
                    await _redisService.PublishAsync(channel, record);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RegistrationImport - Publish realtime lỗi: " + record.PlateNumber + " - " + ex.Message);
            }

            if (string.IsNullOrEmpty(record.AudioPath))
            {
                if (!_workQueueClient.SyncQueue(record))
                {
                    _workQueueClient.SyncQueue(record);
                }
            }
        }

        /// <summary>
        /// Cut-off lấy động từ AllCode.TIME_RESET (màn Configuration). Không đọc được → 17:55.
        /// </summary>
        private async Task<string> ResolveRealtimeChannelAsync(DateTime registrationTime)
        {
            string factoryChannel = _configuration["RegistrationConsumer:RealtimeChannel"] ?? "Add_ReceiveRegistration_BinhDinh";
            string registeredChannel = _configuration["RegistrationConsumer:RealtimeChannelRegistered"]
                                       ?? (factoryChannel.EndsWith("_DK", StringComparison.Ordinal)
                                           ? factoryChannel
                                           : factoryChannel + "_DK");

            var cutoff = await GetTodayCutoffAsync();
            // Sau giờ cut-off hôm nay (và trước nửa đêm) → chỉ hiện ở danh sách đăng ký online
            if (registrationTime >= cutoff) return registeredChannel;
            return factoryChannel;
        }

        private async Task<DateTime> GetTodayCutoffAsync()
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
                LogHelper.InsertLogTelegram("RegistrationImport - đọc TIME_RESET lỗi: " + ex.Message);
            }

            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
        }
    }
}
