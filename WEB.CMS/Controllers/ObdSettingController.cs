using Microsoft.AspNetCore.Mvc;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ObdSettingController : Controller
    {
        private const string VIEW_FOLDER = "~/Views/OcbSetting/";

        public IActionResult Index()
        {
            ViewData["Title"] = "Cấu hình OBD";
            return View(VIEW_FOLDER + "Index.cshtml");
        }
    }
}
