using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Services.Registration;

namespace XTECH_FRONTEND.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoService _mongoService;
        private readonly IRegistrationScheduleService _scheduleService;
        public HomeController(IMongoService mongoService, IRegistrationScheduleService scheduleService)
        {
            _mongoService = mongoService;
            _scheduleService = scheduleService;
        }
        // GET: /home/
        public async Task<IActionResult> Index()
        {
            // Giờ reset lấy từ AllCode.TIME_RESET (CMS cấu hình), view dùng để khóa form trước giờ chốt sổ
            var resetTime = await _scheduleService.GetTodayResetTimeAsync();
            ViewBag.Data = resetTime.ToString("dd/MM/yyyy HH:mm:ss");
            return View();
        }
        public IActionResult ListData()
        {
            var data = _mongoService.GetList();
            ViewBag.Data = data;
            return View();
        }
        //home/welcome?a=hello&b=2
        public string Welcome(string a,int  b=1)
        {
            return HtmlEncoder.Default.Encode($"Hello {a}, NumTimes is: {b}");
        }       

    }
}
