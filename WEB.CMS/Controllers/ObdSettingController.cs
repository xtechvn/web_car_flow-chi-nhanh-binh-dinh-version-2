using Entities.ViewModels.ObdConfig;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Globalization;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ObdSettingController : Controller
    {
        private const string VIEW_FOLDER = "~/Views/OcbSetting/";
        private const long IMPORT_MAX_FILE_SIZE = 5 * 1024 * 1024;
        private static readonly int[] PAGE_SIZES = { 20, 50, 100 };

        private readonly IObdConfigRepository _obdConfigRepository;

        public ObdSettingController(IObdConfigRepository obdConfigRepository)
        {
            _obdConfigRepository = obdConfigRepository;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Cấu hình OBD";
            var businessDate = await _obdConfigRepository.GetBusinessDate();
            ViewBag.BusinessDate = businessDate.ToString("yyyy-MM-dd");
            ViewBag.ImportMaxRows = ObdConfigRepository.IMPORT_MAX_ROWS;
            ViewBag.ImportMaxFileSizeMb = IMPORT_MAX_FILE_SIZE / 1024 / 1024;
            return View(VIEW_FOLDER + "Index.cshtml");
        }

        /// <summary>
        /// Thông tin version của ngày đang chọn: danh sách version, 3 chỉ số thống kê, version mới nhất.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetSummary(ObdConfigSearchModel searchModel)
        {
            try
            {
                var businessDate = await _obdConfigRepository.GetBusinessDate();
                var loadingDate = ParseDate(searchModel?.LoadingDate) ?? businessDate;
                var versions = await _obdConfigRepository.GetListVersion(loadingDate);
                var latest = versions.OrderByDescending(s => s.Version).FirstOrDefault();
                var selected = searchModel?.Version != null
                    ? versions.FirstOrDefault(s => s.Version == searchModel.Version.Value) ?? latest
                    : latest;

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new
                    {
                        loadingDate = loadingDate.ToString("yyyy-MM-dd"),
                        loadingDateText = loadingDate.ToString("dd/MM/yyyy"),
                        businessDate = businessDate.ToString("yyyy-MM-dd"),
                        isBusinessDate = loadingDate.Date == businessDate.Date,
                        totalVersion = versions.Count,
                        latestVersion = latest?.Version ?? 0,
                        selectedVersion = selected?.Version ?? 0,
                        isLatest = selected != null && latest != null && selected.Version == latest.Version,
                        totalObd = selected?.TotalObd ?? 0,
                        totalNetWeight = selected?.TotalNetWeight ?? 0,
                        totalNetWeightText = FormatWeight(selected?.TotalNetWeight ?? 0),
                        lastUpdateText = selected?.LastUpdate?.ToString("HH:mm dd/MM/yyyy") ?? "",
                        versions = versions.Select(s => new { version = s.Version, totalRow = s.TotalRow })
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSummary - ObdSettingController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.ERROR, msg = "Không tải được thông tin version, vui lòng thử lại" });
        }

        [HttpPost]
        public async Task<IActionResult> GetList(ObdConfigSearchModel searchModel)
        {
            var data = new List<ObdConfigModel>();
            searchModel ??= new ObdConfigSearchModel();
            if (!PAGE_SIZES.Contains(searchModel.PageSize)) searchModel.PageSize = PAGE_SIZES[0];
            if (searchModel.PageIndex < 1) searchModel.PageIndex = 1;
            try
            {
                var loadingDate = ParseDate(searchModel.LoadingDate) ?? await _obdConfigRepository.GetBusinessDate();
                if (searchModel.Version.HasValue && searchModel.Version.Value > 0)
                {
                    data = await _obdConfigRepository.GetList(loadingDate, searchModel.Version.Value, searchModel.Delivery, searchModel.PageIndex, searchModel.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - ObdSettingController: " + ex);
            }
            ViewBag.PageIndex = searchModel.PageIndex;
            ViewBag.PageSize = searchModel.PageSize;
            ViewBag.TotalRow = data.FirstOrDefault()?.TotalRow ?? 0;
            return PartialView(VIEW_FOLDER + "GetList.cshtml", data);
        }

        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Vui lòng chọn file Excel cần import" });
                }
                if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Chỉ hỗ trợ file Excel định dạng .xlsx" });
                }
                if (file.Length > IMPORT_MAX_FILE_SIZE)
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = $"File vượt quá dung lượng cho phép {IMPORT_MAX_FILE_SIZE / 1024 / 1024}MB" });
                }

                ObdConfigImportResult importResult;
                using (var stream = file.OpenReadStream())
                {
                    importResult = _obdConfigRepository.ReadImportFile(stream);
                }
                if (!importResult.IsValid)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "File import không hợp lệ, vui lòng sửa theo danh sách lỗi rồi import lại",
                        errors = importResult.Errors
                    });
                }

                var save = await _obdConfigRepository.InsertFromExcel(importResult.Rows, GetUserId(), Path.GetFileName(file.FileName));
                if (save != null && save.Result > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = $"Import thành công {save.TotalRow:N0} dòng vào Version {save.Version} ngày {save.LoadingDate:dd/MM/yyyy}",
                        loadingDate = save.LoadingDate?.ToString("yyyy-MM-dd"),
                        version = save.Version
                    });
                }
                return Ok(new { status = (int)ResponseType.FAILED, msg = save?.Message ?? "Import không thành công, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Import - ObdSettingController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.ERROR, msg = "Đã có lỗi xảy ra, vui lòng liên hệ IT" });
        }

        [HttpPost]
        public async Task<IActionResult> Insert(string delivery, string netWeight)
        {
            try
            {
                if (!ObdValueParser.TryParseDelivery(delivery, out var deliveryValue))
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = $"Delivery không hợp lệ (chỉ gồm chữ số, tối đa {ObdValueParser.DELIVERY_MAX_LENGTH} số)" });
                }
                if (!ObdValueParser.TryParseNetWeight(netWeight, out var netWeightValue))
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Net Weight không hợp lệ (phải là số không âm)" });
                }

                var save = await _obdConfigRepository.Insert(deliveryValue, netWeightValue, GetUserId());
                if (save != null && save.Result > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = $"Đã thêm vào Version {save.Version} ngày {save.LoadingDate:dd/MM/yyyy}",
                        loadingDate = save.LoadingDate?.ToString("yyyy-MM-dd"),
                        version = save.Version,
                        row = new
                        {
                            obdId = save.ObdId,
                            delivery = deliveryValue.ToString(),
                            loadingDateText = save.LoadingDate?.ToString("dd/MM/yyyy"),
                            version = save.Version,
                            netWeightText = FormatWeight(netWeightValue),
                            userCreateName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? ""
                        }
                    });
                }
                return Ok(new { status = (int)ResponseType.FAILED, msg = save?.Message ?? "Thêm mới không thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - ObdSettingController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.ERROR, msg = "Đã có lỗi xảy ra, vui lòng liên hệ IT" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDelivery(long obdId, string delivery)
        {
            try
            {
                if (obdId <= 0)
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Bản ghi không hợp lệ" });
                }
                if (!ObdValueParser.TryParseDelivery(delivery, out var deliveryValue))
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = $"Delivery không hợp lệ (chỉ gồm chữ số, tối đa {ObdValueParser.DELIVERY_MAX_LENGTH} số)" });
                }

                var save = await _obdConfigRepository.UpdateDelivery(obdId, deliveryValue, GetUserId());
                if (save != null && save.Result > 0)
                {
                    return Ok(new { status = (int)ResponseType.SUCCESS, msg = save.Message });
                }
                return Ok(new { status = (int)ResponseType.FAILED, msg = save?.Message ?? "Cập nhật không thành công" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateDelivery - ObdSettingController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.ERROR, msg = "Đã có lỗi xảy ra, vui lòng liên hệ IT" });
        }

        [HttpPost]
        public IActionResult History(long obdId)
        {
            ObdConfigHistoryModel? model = null;
            try
            {
                if (obdId > 0) model = _obdConfigRepository.GetHistory(obdId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("History - ObdSettingController: " + ex);
            }
            return PartialView(VIEW_FOLDER + "History.cshtml", model);
        }

        private int GetUserId()
        {
            var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? Convert.ToInt32(claim.Value) : 0;
        }

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var formats = new[] { "yyyy-MM-dd", "dd/MM/yyyy" };
            return DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.Date
                : null;
        }

        public static string FormatWeight(decimal value)
        {
            return value.ToString("#,##0.###", CultureInfo.InvariantCulture);
        }
    }
}
