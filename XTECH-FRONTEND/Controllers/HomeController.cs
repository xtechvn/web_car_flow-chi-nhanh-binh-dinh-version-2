using Entities.ViewModels;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Xml.Linq;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMongoService _mongoService;
        public HomeController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }
        // GET: /home/
        public async Task<IActionResult> Index()
        {
            var request = new CarRegistrationResponse();
            string url = "http://api.longancargill.com/api/vehicleInspection/get-time-countdown?LocationType=3";
            var client = new HttpClient();
            var request_api = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request_api);
            if (response.IsSuccessStatusCode)
            {
                var text = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<ApiCountdownModel>(text);
                ViewBag.Data = result.data;
            }
            else
            {
                LogHelper.InsertLogTelegram("Insert - lỗi ");
            }
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
