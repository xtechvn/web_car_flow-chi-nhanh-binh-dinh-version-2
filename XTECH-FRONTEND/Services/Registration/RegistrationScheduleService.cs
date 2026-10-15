using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Globalization;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.Registration
{
    /// <summary>
    /// Đọc giờ reset (AllCode.TIME_RESET, do CMS cấu hình tại /Configuration) từ SQL Server
    /// và tính đợt cấp số thứ tự ở phía server.
    /// </summary>
    public class RegistrationScheduleService : IRegistrationScheduleService
    {
        private const string CacheKey = "registration:reset_time_of_day";

        private readonly IMemoryCache _cache;
        private readonly RegistrationOptions _options;
        private readonly string? _connectionString;
        private readonly ILogger<RegistrationScheduleService> _logger;

        public RegistrationScheduleService(
            IMemoryCache cache,
            IOptions<RegistrationOptions> options,
            IConfiguration configuration,
            ILogger<RegistrationScheduleService> logger)
        {
            _cache = cache;
            _options = options.Value;
            _connectionString = configuration["DataBaseConfig:SqlServer:ConnectionString"];
            _logger = logger;
        }

        public async Task<DateTime> GetTodayResetTimeAsync()
        {
            var resetTimeOfDay = await GetResetTimeOfDayAsync();
            return DateTime.Today.Add(resetTimeOfDay);
        }

        public async Task<RegistrationWindow> GetCurrentWindowAsync(DateTime now)
        {
            var resetTimeOfDay = await GetResetTimeOfDayAsync();

            // Giữ quy tắc gốc: số thứ tự về 1 lúc (TIME_RESET - 5 phút), không chặn đăng ký
            var periodEnd = now.Date.Add(resetTimeOfDay).AddMinutes(-_options.CounterResetOffsetMinutes);
            if (now >= periodEnd)
            {
                periodEnd = periodEnd.AddDays(1);
            }

            return new RegistrationWindow
            {
                BusinessDate = periodEnd.Date,
                PeriodStart = periodEnd.AddDays(-1),
                PeriodEnd = periodEnd
            };
        }

        private async Task<TimeSpan> GetResetTimeOfDayAsync()
        {
            if (_cache.TryGetValue(CacheKey, out TimeSpan cached))
            {
                return cached;
            }

            var resetTimeOfDay = await ReadResetTimeFromSqlAsync() ?? ParseDefaultResetTime();
            _cache.Set(CacheKey, resetTimeOfDay, TimeSpan.FromSeconds(_options.ResetTimeCacheSeconds));
            return resetTimeOfDay;
        }

        private async Task<TimeSpan?> ReadResetTimeFromSqlAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                return null;
            }

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new SqlCommand(
                    "SELECT TOP 1 UpdateTime FROM AllCode WHERE Type = @Type ORDER BY Description", connection);
                command.Parameters.AddWithValue("@Type", "TIME_RESET");

                var value = await command.ExecuteScalarAsync();
                if (value is DateTime updateTime)
                {
                    return new TimeSpan(updateTime.Hour, updateTime.Minute, 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không đọc được AllCode.TIME_RESET, dùng giờ mặc định");
                _ = Task.Run(() => LogHelper.InsertLogTelegram("RegistrationScheduleService - đọc TIME_RESET lỗi: " + ex.Message));
            }

            return null;
        }

        private TimeSpan ParseDefaultResetTime()
        {
            return TimeSpan.TryParseExact(_options.DefaultResetTime, @"hh\:mm", CultureInfo.InvariantCulture, out var value)
                ? value
                : new TimeSpan(17, 55, 0);
        }
    }
}
