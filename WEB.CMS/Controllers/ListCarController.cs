using Entities.Models;
using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Repositories.IRepositories;
using Repositories.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ListCarController : Controller
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;

        public ListCarController(IVehicleInspectionRepository vehicleInspectionRepository, IAllCodeRepository allCodeRepository)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;
        }
        public IActionResult CarCallList()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CartoFactory - CarController: " + ex);
            }
            return View();
        }
        public IActionResult ListVehicles()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CartoFactory - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListCarCallView(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                ViewBag.AllCode = AllCode;
                var AllCode2 = await _allCodeRepository.GetListSortByName(AllCodeType.TROUGH_TYPE);
                ViewBag.AllCode2 = AllCode2;
                var data = await _vehicleInspectionRepository.GetListVehicleCarCallList(SearchModel);
               
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        item.ListTroughWeight = await _vehicleInspectionRepository.GetListTroughWeightByVehicleInspectionId(item.Id);
                        if ((item.ListTroughWeight == null ||item.VehicleTroughStatus != (int)VehicleTroughStatus.Bo_Luot) && SearchModel.type == 0 )
                        {
                            if (item.ListTroughWeight == null)
                            {
                                var list = new List<TroughWeight>();
                                var detail = new TroughWeight();
                                list.Add(detail);
                                item.ListTroughWeight= list;
                            } 
                            else
                                item.ListTroughWeight.Add(new TroughWeight());
                        }
                    }
                }

                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCartoFactory - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> ListVehiclesisLoading(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHINGSTATUS);
                ViewBag.AllCode = AllCode;
                var data = await _vehicleInspectionRepository.GetListVehicleListVehicles(SearchModel);
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        item.ListTroughWeight = await _vehicleInspectionRepository.GetListTroughWeightByVehicleInspectionId(item.Id);
                       
                    }
                }
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListProcessingIsLoading - CarController: " + ex);
            }
            return PartialView();
        }

    }
}
