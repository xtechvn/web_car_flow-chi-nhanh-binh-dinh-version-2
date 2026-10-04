using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;


namespace XTECH_FRONTEND.Repositories
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<GoogleSheetsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SheetsService _sheetsService;

        private readonly string _spreadsheetId;
        private readonly string _sheetName;

        public GoogleSheetsService(
            IMemoryCache cache,
            ILogger<GoogleSheetsService> logger,
            IConfiguration configuration)
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;

            // Get configuration
            _spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"]
                ?? "1mocqFI7Gue7E47K3LjhPTCYbEt7Rl-Gw1MrchDHk_dA";
            _sheetName = _configuration["GoogleSheets:SheetName"] ?? "Sheet1";

            // Initialize Google Sheets service
            _sheetsService = InitializeSheetsService();
        }

        private SheetsService InitializeSheetsService()
        {
            try
            {
                var serviceAccountFile = _configuration["GoogleSheets:ServiceAccountFile"];
                if (!string.IsNullOrEmpty(serviceAccountFile) && File.Exists(serviceAccountFile))
                {
                    var credential = GoogleCredential.FromFile(serviceAccountFile)
                        .CreateScoped(SheetsService.Scope.Spreadsheets);

                    return new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Car Registration API"
                    });
                }

                var serviceAccountJson = _configuration["GoogleSheets:ServiceAccountJson"];
                if (!string.IsNullOrEmpty(serviceAccountJson))
                {
                    var credential = GoogleCredential.FromJson(serviceAccountJson)
                        .CreateScoped(SheetsService.Scope.Spreadsheets);

                    return new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Car Registration API"
                    });
                }

                throw new InvalidOperationException("Google Sheets credentials not configured properly");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Sheets service");
                throw;
            }
        }

        public async Task<int> GetDailyQueueCountAsync()
        {
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM");
                var cacheKey = $"daily_count_{today}";
                var todayStart = DateTime.Today;
                var cutoffTime = todayStart.AddHours(18); // 18:00 hôm nay
                var tomorrowStart = todayStart.AddDays(1);
                if (DateTime.Now >= cutoffTime)
                {
                    _cache.Remove(cacheKey);
                }
                var range = $"{_sheetName}!A:H"; // Updated to include Zalo Status column
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

                var response = await request.ExecuteAsync();
                var values = response.Values;

                if (values == null || values.Count <= 1)
                {
                    _logger.LogInformation("No registration data found for today");
                    _cache.Set(cacheKey, 1);
                    return 0;
                }
                if (values.Count == 2)
                {
                    _cache.Remove(cacheKey);
                }
                if (_cache.TryGetValue(cacheKey, out int cachedCount))
                {
                    _logger.LogInformation($"Retrieved daily queue count from cache: {cachedCount}");
                    _cache.Set(cacheKey, cachedCount + 1);
                    return cachedCount;
                }

                var count = 0;
                for (int i = 1; i < values.Count; i++)
                {
                    var row = values[i];
                    if (row.Count >= 6 && !string.IsNullOrEmpty(row[6]?.ToString()))
                    {
                        if (DateTime.TryParse(row[6].ToString(), out DateTime registrationDate))
                        {
                            if (registrationDate.Date.AddDays(1) >= todayStart.Date)
                            {
                                count++;
                            }
                        }
                    }
                }

                _logger.LogInformation($"Retrieved daily queue count from Google Sheets: {count}");
                if (count == 0)
                {
                    _cache.Set(cacheKey, 1);
                }
                else
                {
                    _cache.Set(cacheKey, count + 1);
                }


                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily queue count from Google Sheets");

                var today = DateTime.Today.ToString("yyyy-MM");
                var cacheKey = $"daily_count_{today}";

                if (_cache.TryGetValue(cacheKey, out int cachedCount))
                {
                    _logger.LogWarning("Returning cached value due to error");
                    return cachedCount;
                }

                throw;
            }
        }

        public async Task<bool> SaveRegistrationAsync(RegistrationRecord record)
        {
            try
            {
                // Updated to include Zalo Status column
                var values = new List<IList<object>>
                {
                    new List<object>
                    {
                        record.Name,
                        record.PlateNumber,
                        record.GPLX,
                        record.Referee,
                        record.PhoneNumber,
                        record.QueueNumber,
                        record.RegistrationTime.ToString("yyyy-MM-dd HH:mm:ss").ToString(),
                        record.ZaloStatus,
                        record.Camp
                    }
                };

                var valueRange = new ValueRange
                {
                    Values = values
                };

                var appendRequest = _sheetsService.Spreadsheets.Values.Append(
                    valueRange,
                    _spreadsheetId,
                    $"{_sheetName}!A:E");

                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                var appendResponse = await appendRequest.ExecuteAsync();


                // 2. Lấy dòng vừa chèn
                var updatedRange = appendResponse.Updates.UpdatedRange; // ví dụ: "Sheet1!A10:E10"
                var startRow = int.Parse(Regex.Match(updatedRange, @"[A-Z]+(\d+)").Groups[1].Value) - 1;

                // 3. Lấy SheetId (không phải tên)
                var spreadsheet = await _sheetsService.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == _sheetName);
                var sheetId = sheet?.Properties.SheetId;

                if (sheetId == null)
                {
                    throw new Exception("Không tìm thấy SheetId.");
                }
                // 4. Gửi BatchUpdate để định dạng dòng
                var formatRequest = new BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Request>
                    {
                        new Request
                        {
                            RepeatCell = new RepeatCellRequest
                            {
                                Range = new GridRange
                                {
                                    SheetId = sheetId,
                                    StartRowIndex = startRow,
                                    EndRowIndex = startRow + 1,
                                    StartColumnIndex = 0,
                                    EndColumnIndex = 9 // Cột A đến I (0 đến 8)
                                },
                                Cell = new CellData
                                {
                                    UserEnteredFormat = new CellFormat
                                    {
                                        BackgroundColor = new Color
                                        {
                                            Red = 1.0f, Green = 1.0f, Blue = 1.0f // Trắng
                                        },
                                        TextFormat = new TextFormat
                                        {
                                            ForegroundColor = new Color
                                            {
                                               Red = 0.0f, Green = 0.0f, Blue = 0.0f // Đen
                                            },
                                            Bold = true
                                        }
                                    }
                                },
                                Fields = "userEnteredFormat(backgroundColor,textFormat)"
                            }
                        }
                    }
                };

                var batchRequest = _sheetsService.Spreadsheets.BatchUpdate(formatRequest, _spreadsheetId);
                await batchRequest.ExecuteAsync();
                if (appendResponse.Updates.UpdatedRows.HasValue && appendResponse.Updates.UpdatedRows.Value > 0)
                {
                    _logger.LogInformation($"Successfully saved registration to Google Sheets: {record.PhoneNumber} - {record.PlateNumber} - Queue: {record.QueueNumber} - Zalo: {record.ZaloStatus}- Camp: {record.Camp}");

                    //var today = DateTime.Today.ToString("yyyy-MM");
                    //var cacheKey = $"daily_count_{today}";
                    //if (_cache.TryGetValue(cacheKey, out int currentCount))
                    //{
                    //    _cache.Set(cacheKey, currentCount + 1);
                    //}

                    return true;
                }
                _logger.LogWarning("No rows were updated when saving to Google Sheets");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving registration to Google Sheets");
                return false;
            }
        }

        public async Task<DateTime?> GetLastSubmissionTimeAsync(string phoneNumber)
        {
            try
            {
                var cacheKey = $"last_submission_{phoneNumber}";

                if (_cache.TryGetValue(cacheKey, out DateTime cachedTime))
                {
                    return cachedTime;
                }

                var range = $"{_sheetName}!A:E";
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

                var response = await request.ExecuteAsync();
                var values = response.Values;

                if (values == null || values.Count <= 1)
                {
                    return null;
                }

                DateTime? lastSubmission = null;

                for (int i = values.Count - 1; i >= 1; i--)
                {
                    var row = values[i];

                    if (row.Count >= 6 &&
                        row[0]?.ToString() == phoneNumber &&
                        !string.IsNullOrEmpty(row[6]?.ToString()))
                    {
                        if (DateTime.TryParse(row[6].ToString(), out DateTime submissionTime))
                        {
                            lastSubmission = submissionTime;
                            break;
                        }
                    }
                }

                if (lastSubmission.HasValue)
                {
                    _cache.Set(cacheKey, lastSubmission.Value, TimeSpan.FromMinutes(15));
                    _logger.LogInformation($"Found last submission for {phoneNumber}: {lastSubmission}");
                }

                return lastSubmission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting last submission time for {phoneNumber}");
                return null;
            }
        }

        public async Task UpdateLastSubmissionTimeAsync(string PlateNumber, DateTime submissionTime)
        {
            try
            {
                var cacheKey = $"last_submission_{PlateNumber.Replace("-", "")}";
                _cache.Set(cacheKey, submissionTime, TimeSpan.FromMinutes(15));

                _logger.LogInformation($"Updated last submission time for {PlateNumber}: {submissionTime}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating last submission time for {PlateNumber}");
                throw;
            }
        }

        public async Task<bool> EnsureSheetHeadersAsync()
        {
            try
            {
                var range = $"{_sheetName}!A1:E1"; // Updated to include Zalo Status column
                var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

                var response = await request.ExecuteAsync();

                if (response.Values == null || response.Values.Count == 0)
                {
                    var headers = new List<IList<object>>
                    {
                        new List<object> { "Tên khách hàng(Trại hoặc đại lý)", "Biển số xe đăng ký", "Số GPLX(3 số cuối giấy phép lái xe)", "Trọng tải xe", "Số điện thoại tài xế", "Số thứ tự", "Ngày giờ đăng ký", "Trạng thái gửi Zalo", "Hoàn hảo/Trại" }
                    };

                    var valueRange = new ValueRange { Values = headers };

                    var updateRequest = _sheetsService.Spreadsheets.Values.Update(
                        valueRange,
                        _spreadsheetId,
                        range);

                    updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                    await updateRequest.ExecuteAsync();
                    _logger.LogInformation("Created headers in Google Sheet with Zalo Status column");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring sheet headers");
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sheetsService?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public async Task<int> GetDailyQueueCountRedis(DateTime? time)
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(_configuration["Redis:Host"] +":"+_configuration["Redis:Port"]);
                var db = redis.GetDatabase();
                // Tính effective date dựa trên giờ địa phương (UTC+7)
                DateTime now = DateTime.Now; // Sử dụng giờ hệ thống (giả định đã cấu hình đúng timezone)
                string key = $"counter:daily_car_count_Pro_Binh_Dinh";
             
                long nextNumber = db.StringIncrement(key);
                var datetime_5p= time?.AddMinutes(-5);
                // Đặt TTL nếu là lần đầu tăng
                // 🔹 1. Nếu có time truyền vào → xử lý reset theo time
                if (nextNumber == 1)
                {
                    // Mục tiêu: 18 hôm nay
                    DateTime expireAt = new DateTime(now.Year, now.Month, now.Day, datetime_5p.Value.Hour, datetime_5p.Value.Minute, 00);

                    // Nếu đã quá 18 hôm nay → chuyển sang 18 ngày mai
                    if (now > expireAt)
                    {
                        expireAt = expireAt.AddDays(1);
                    }

                    TimeSpan ttl = expireAt - now;
                    db.KeyExpire(key, ttl);
                }
                Console.WriteLine($"Số thứ tự tiếp theo: {nextNumber}");
                return (int)nextNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily queue count from Google Sheets");

                throw;
            }
        }
    }
}
