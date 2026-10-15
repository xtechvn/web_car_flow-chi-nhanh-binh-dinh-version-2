using Microsoft.AspNetCore.Mvc;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers
{
    /// <summary>
    /// Xe đăng ký có trong MongoDB nhưng chưa vào SQL: đếm (icon nhấp nháy), xem danh sách (lightbox) và đẩy lại.
    /// </summary>
    [CustomAuthorize]
    public class RegistrationRecoveryController : Controller
    {
        private readonly RegistrationRecoveryService _recoveryService;

        public RegistrationRecoveryController(RegistrationRecoveryService recoveryService)
        {
            _recoveryService = recoveryService;
        }

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            try
            {
                var missing = await _recoveryService.GetMissingAsync();
                return Ok(new { status = (int)ResponseType.SUCCESS, count = missing.Count });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Count - RegistrationRecoveryController: " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, count = 0 });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Popup()
        {
            try
            {
                ViewBag.Period = await _recoveryService.GetCurrentPeriodAsync();
                var missing = await _recoveryService.GetMissingAsync();
                return PartialView(missing);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Popup - RegistrationRecoveryController: " + ex);
                ViewBag.Error = "Không đọc được dữ liệu MongoDB/SQL, vui lòng thử lại.";
                return PartialView(new List<MissingRegistration>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Push(List<string> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Vui lòng chọn ít nhất một xe để đẩy." });
                }

                var result = await _recoveryService.PushAsync(ids);
                var msg = $"Đã đẩy {result.Pushed} xe vào hệ thống.";
                if (result.Skipped > 0) msg += $" {result.Skipped} xe đã có sẵn nên bỏ qua.";
                if (result.Failed.Count > 0) msg += " Lỗi: " + string.Join(", ", result.Failed) + ".";

                return Ok(new
                {
                    status = result.Failed.Count > 0 ? (int)ResponseType.FAILED : (int)ResponseType.SUCCESS,
                    msg,
                    pushed = result.Pushed,
                    skipped = result.Skipped,
                    failed = result.Failed,
                    remaining = result.Remaining
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Push - RegistrationRecoveryController: " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Lỗi hệ thống, vui lòng thử lại." });
            }
        }
    }
}
