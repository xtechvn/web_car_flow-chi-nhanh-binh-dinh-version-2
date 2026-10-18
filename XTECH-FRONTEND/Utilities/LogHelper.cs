using Newtonsoft.Json;
using Telegram.Bot;

namespace XTECH_FRONTEND.Utilities
{
    public static class LogHelper
    {
        public static string botToken = "8199447284:AAHMs8h2hJrG0XU75SrmWJP0Dkpie77uNYE";
        public static string group_Id = "-4715213980";
        public static string enviromment = "DEV-CAR";
        public static string CompanyType = " ";
        public static int CompanyTypeInt = 0;

        private static readonly object ConfigLock = new object();
        private static DateTime _configLoadedAt = DateTime.MinValue;

        private static void LoadConfig()
        {
            // IIS hay đổi thư mục làm việc → đọc tương đối "appsettings.json" dễ thất bại, log Telegram im lặng
            if ((DateTime.UtcNow - _configLoadedAt).TotalSeconds < 60) return;

            lock (ConfigLock)
            {
                if ((DateTime.UtcNow - _configLoadedAt).TotalSeconds < 60) return;
                try
                {
                    var path = ResolveAppSettingsPath();
                    if (path == null) return;

                    var json = File.ReadAllText(path);
                    var _appconfig = JsonConvert.DeserializeObject<AppSettings>(json);
                    if (_appconfig?.BotSetting == null) return;

                    enviromment = _appconfig.BotSetting.environment ?? enviromment;
                    botToken = string.IsNullOrWhiteSpace(_appconfig.BotSetting.bot_token)
                        ? botToken
                        : _appconfig.BotSetting.bot_token.Trim();
                    group_Id = string.IsNullOrWhiteSpace(_appconfig.BotSetting.bot_group_id)
                        ? group_Id
                        : _appconfig.BotSetting.bot_group_id.Trim();
                    CompanyType = _appconfig.CompanyType ?? CompanyType;
                    _configLoadedAt = DateTime.UtcNow;
                }
                catch
                {
                    // Giữ cấu hình đã có / mặc định
                }
            }
        }

        private static string? ResolveAppSettingsPath()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                "appsettings.json"
            };
            return candidates.FirstOrDefault(File.Exists);
        }

        public static int InsertLogTelegram(string message)
        {
            var rs = 1;
            try
            {
                LoadConfig();
                var text = "[" + enviromment + "-" + CompanyType + "] - " + (message ?? string.Empty);
                // Telegram giới hạn 4096 ký tự mỗi tin
                if (text.Length > 4000) text = text.Substring(0, 3990) + "...(cắt)";

                var alertMsgBot = new TelegramBotClient(botToken);
                var rs_push = alertMsgBot.SendTextMessageAsync(group_Id.Trim(), text).Result;
            }
            catch (Exception ex)
            {
                rs = -1;
                WriteFallback("Telegram lỗi: " + ex.Message + " | Nội dung gốc: " + message);
            }
            return rs;
        }

        /// <summary>Gửi lỗi đăng ký lên Telegram kèm biển số / IP / stack (đã cắt ngắn).</summary>
        public static void InsertRegistrationError(string source, string? plate, string? ip, Exception ex)
        {
            var msg =
                source + "\n"
                + "Biển: " + (string.IsNullOrWhiteSpace(plate) ? "(không có)" : plate) + "\n"
                + "IP: " + (string.IsNullOrWhiteSpace(ip) ? "(không có)" : ip) + "\n"
                + "Lỗi: " + ex.GetType().Name + ": " + ex.Message + "\n"
                + "Inner: " + (ex.InnerException?.Message ?? "(không)") + "\n"
                + "Stack: " + (ex.StackTrace ?? "");
            _ = Task.Run(() => InsertLogTelegram(msg));
        }

        private static void WriteFallback(string content)
        {
            try
            {
                var dir = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(
                    Path.Combine(dir, "telegram_fail.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + content + Environment.NewLine);
            }
            catch
            {
            }
        }
    }

    public class AppSettings
    {
        public BotSetting BotSetting { get; set; }
        public string CompanyType { get; set; }

    }

    public class BotSetting
    {
        public string bot_token { get; set; }
        public string bot_group_id { get; set; }
        public string environment { get; set; }
    }
    public class SystemLog
    {

        public int SourceID { get; set; }
        public string Type { get; set; }
        public string KeyID { get; set; }
        public string ObjectType { get; set; }
        public int CompanyType { get; set; }
        public string Log { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
