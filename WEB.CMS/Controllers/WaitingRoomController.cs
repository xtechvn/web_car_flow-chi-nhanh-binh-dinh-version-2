using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers
{
    public class WaitingRoomController : Controller
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        public WaitingRoomController(IVehicleInspectionRepository vehicleInspectionRepository, IAllCodeRepository allCodeRepository)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CarCall()
        {
            return View();
        }
        public async Task<IActionResult> GetList(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.LOADINGSTATUS);
                ViewBag.AllCode = AllCode;
                var AllCode2 = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                ViewBag.AllCode2 = AllCode2;
                var AllCode3 = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                ViewBag.AllCode3 = AllCode3;
                var AllCode4 = await _allCodeRepository.GetListSortByName(AllCodeType.Rank_Type);
                ViewBag.AllCode4 = AllCode4;
                var data = await _vehicleInspectionRepository.GetListVehicleProcessingIsLoading(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - WaitingRoomController: " + ex);
            }
            return PartialView();
        }
    }
}
