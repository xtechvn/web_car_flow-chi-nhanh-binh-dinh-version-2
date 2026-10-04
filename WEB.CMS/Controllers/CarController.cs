using Entities.Models;
using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Nest;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using PuppeteerSharp;
using Repositories.IRepositories;
using System.Security.Claims;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class CarController : Controller
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly IHubContext<CarHub> _hubContext;
        private readonly WorkQueueClient _workQueueClient;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public CarController(IVehicleInspectionRepository vehicleInspectionRepository, IAllCodeRepository allCodeRepository, IHubContext<CarHub> hubContext, IConfiguration configuration, IWebHostEnvironment WebHostEnvironment)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _allCodeRepository = allCodeRepository;
            _hubContext = hubContext;
            _workQueueClient = new WorkQueueClient(configuration);
            _WebHostEnvironment = WebHostEnvironment;
        }
        public IActionResult CartoFactory()
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
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                return PartialView(detail);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProcessingIsLoading - CarController: " + ex);
            }
            return View();
        }

        public async Task<IActionResult> ListCartoFactory(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;//1 đã SL
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                var LoadingType = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                ViewBag.AllCode = AllCode;
                ViewBag.LoadingType = LoadingType;
                var data = await _vehicleInspectionRepository.GetListCartoFactory(SearchModel);
                if (data != null && data.Count > 0 && SearchModel.type == 1)
                {
                    data = data.OrderBy(s => s.VehicleArrivalDate).ToList();
                }
                ViewBag.count = data != null ? data.Count : 0;
                ViewBag.sum = data != null ? data.Sum(x => Convert.ToInt32(x.VehicleLoad)).ToString("N0") : "0";
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCartoFactory - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> OpenPopup(int id, int type)
        {
            try
            {
                ViewBag.Id = id;
                ViewBag.StatusCar = 0;
                var data = new List<AllCode>();
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                switch (type)
                {
                    case 1:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                        ViewBag.StatusCar = detail.VehicleStatus;
                        break;
                    case 2:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                        ViewBag.StatusCar = detail.LoadType;
                        break;
                    case 3:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                        ViewBag.StatusCar = detail.VehicleWeighingType;
                        break;
                    case 4:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.TROUGH_TYPE);
                        ViewBag.StatusCar = detail.TroughType;
                        break;
                    case 5:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUG_HWEIGHT);
                        ViewBag.StatusCar = detail.VehicleTroughWeight;
                        break;
                    case 6:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                        ViewBag.StatusCar = detail.VehicleTroughStatus;
                        break;
                    case 7:
                        data = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHINGSTATUS);
                        ViewBag.StatusCar = detail.VehicleWeighingStatus;
                        break;
                }
                ViewBag.Status = data;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OpenPopup - CarController: " + ex);
            }
            return PartialView();
        }
        public IActionResult ProcessingIsLoading()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProcessingIsLoading - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListProcessingIsLoading(CartoFactorySearchModel SearchModel)
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
                LogHelper.InsertLogTelegram("ListProcessingIsLoading - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> CallTheScale()
        {
            try
            {
                var model = new CartoFactorySearchModel()
                {
                    VehicleNumber = "",
                    PhoneNumber = "",
                    VehicleStatus = "0",
                    LoadType = null,
                    VehicleWeighingType = "0",
                    VehicleTroughStatus = null,
                    TroughType = null,
                    VehicleWeighingStatus = null,
                    LoadingStatus = 0,
                    VehicleWeighedstatus = 0,
                    type = 0,
                };
                var data = await _vehicleInspectionRepository.GetListVehicleCarCallList(model);
                ViewBag.input1 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 1).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input2 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 2).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input3 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 3).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input4 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 4).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input5 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 5).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input6 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 6).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input7 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 7).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input8 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 8).ToList().Count <= 0 ? 0 : 1) : 0;
                ViewBag.input9 = data != null && data.Count > 0 ? (data.Where(s => s.TroughType == 9).ToList().Count <= 0 ? 0 : 1) : 0;
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CallTheScale - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListCallTheScale(CartoFactorySearchModel SearchModel)
        {
            try
            {
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                ViewBag.AllCode = AllCode;
                ViewBag.type = SearchModel.type;
                ViewBag.LoadType = SearchModel.LoadType == null ? "" : SearchModel.LoadType;
                var data = await _vehicleInspectionRepository.GetListVehicleCallTheScale(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCallTheScale - CarController: " + ex);
            }
            return PartialView();
        }
        public IActionResult WeighedInput()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListWeighedInput(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHEDSTATUS);
                ViewBag.AllCode = AllCode;
                var data = await _vehicleInspectionRepository.GetListVehicleWeighedInput(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListWeighedInput - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> UpdateStatus(int id, int status, int type, int weight = 0, string Note = null)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var UpdateCar = 0;
                ViewBag.Id = id;
                ViewBag.StatusCar = 0;
                var model = new VehicleInspectionUpdateModel();
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                model.Id = detail.Id;
                model.RecordNumber = detail.RecordNumber;
                model.CustomerName = detail.CustomerName;
                model.VehicleNumber = detail.VehicleNumber;
                model.RegisterDateOnline = detail.RegisterDateOnline;
                model.DriverName = detail.DriverName;
                model.LicenseNumber = detail.LicenseNumber;
                model.PhoneNumber = detail.PhoneNumber;
                model.VehicleLoad = detail.VehicleLoad;
                model.VehicleStatus = detail.VehicleStatus;
                model.LoadType = detail.LoadType;
                model.IssueCreateDate = detail.IssueCreateDate;
                model.IssueUpdatedDate = detail.IssueUpdatedDate;
                model.VehicleWeighingType = detail.VehicleWeighingType;
                model.VehicleWeighingTimeComeIn = detail.VehicleWeighingTimeComeIn;
                model.VehicleWeighingTimeComeOut = detail.VehicleWeighingTimeComeOut;
                model.VehicleWeighingTimeComplete = detail.VehicleWeighingTimeComplete;
                model.TroughType = detail.TroughType;
                model.VehicleTroughTimeComeIn = detail.VehicleTroughTimeComeIn;
                model.VehicleTroughTimeComeOut = detail.VehicleTroughTimeComeOut;
                model.VehicleTroughWeight = detail.VehicleTroughWeight;
                model.VehicleTroughStatus = detail.VehicleTroughStatus;
                model.LoadingStatus = detail.LoadingStatus;
                model.VehicleWeighedstatus = detail.VehicleWeighedstatus;
                model.TimeCallVehicleTroughTimeComeIn = detail.TimeCallVehicleTroughTimeComeIn;
                model.LoadingType = detail.LoadingType;
                model.VehicleArrivalDate = detail.VehicleArrivalDate;
                model.ProcessingIsLoadingDate = detail.ProcessingIsLoadingDate;
                model.VehicleWeightMax = detail.VehicleWeightMax;
                model.VehicleLoadTaken = detail.VehicleLoadTaken;
                model.CreatedBy = _UserId;
                model.ProtectNotes = detail.ProtectNotes;
                model.TrangThai = detail.TrangThai;
                model.Rank = detail.Rank;
                model.RankName = detail.RankName;
                model.CSNotes = detail.CSNotes;
                model.VehicleWeightIn = detail.VehicleWeightIn;
                model.VehicleWeightOut = detail.VehicleWeightOut;
                switch (type)
                {
                    case 1:
                        {
                            if (detail.LoadingStatus == (int)LoadingStatus.Da_HTTC)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã hoàn thành thử tục"
                                });
                            }
                            if (detail.VehicleStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleStatus = status;
                            model.VehicleArrivalDate = DateTime.Now;
                            detail.VehicleArrivalDate = DateTime.Now;
                            model.ProtectNotes = Note;
                            detail.ProtectNotes = Note;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleStatus);
                                detail.VehicleStatusName = allcode_detail.Description;
                                if (status == (int)VehicleStatus.Da_Den_NM)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCartoFactory_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync(status == (int)VehicleStatus.Rac ? "ListCartoFactory_rac" : "ListCartoFactory", detail);

                                }
                            }
                        }
                        break;
                    case 2:
                        {
                            switch (status)
                            {
                                case 1:
                                    {
                                        model.Rank = (int)RankType.Bac;
                                        break;
                                    }
                                case 0:
                                    {
                                        model.Rank = (int)RankType.Vang;
                                        break;
                                    }
                                case 2:
                                    {
                                        model.Rank = (int)RankType.Kim_Cuong;
                                        break;
                                    }
                            }
                            if (weight > 0)
                            {
                                var update = await _vehicleInspectionRepository.UpdateVehicleLoadTaken(id, weight);
                            }
                            model.LoadType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                            var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == status);
                            detail.LoadTypeName = allcode_detail.Description;
                            detail.VehicleLoadTaken = weight;
                            var allcode_Rank = await _allCodeRepository.GetListSortByName(AllCodeType.Rank_Type);
                            var allcode_Rank_detail = allcode_Rank.FirstOrDefault(s => s.CodeValue == model.Rank);
                            detail.RankName = allcode_Rank_detail == null ? "" : allcode_Rank_detail.Description;

                            await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading", detail);

                        }
                        break;
                    case 3:
                        {
                            if (detail.VehicleWeighedstatus == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã cân xong"
                                });
                            }
                            if (status == (int)VehicleWeighingType.DA_Vao_Can)
                            {
                                model.VehicleWeighingTimeComeIn = DateTime.Now;
                            }
                            if (detail.VehicleWeighingType == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleWeighingType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHING_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighingType);
                                detail.VehicleWeighingTypeName = allcode_detail.Description;
                                if (status == (int)VehicleWeighingType.DA_Vao_Can)
                                {
                                    detail.VehicleWeighingTimeComeIn = DateTime.Now;
                                    await _hubContext.Clients.All.SendAsync("ListCallTheScale_Da_SL", detail);
                                }
                                else
                                {
                                    if (detail.LoadType == (int)LoadType.Xanh || detail.LoadType == (int)LoadType.Do)
                                    {
                                        await _hubContext.Clients.All.SendAsync("ListCallTheScale_0", detail);
                                    }
                                    else
                                    {
                                        await _hubContext.Clients.All.SendAsync("ListCallTheScale_1", detail);

                                    }

                                }
                            }



                        }
                        break;
                    case 4:
                        {
                            var VehicleTroughStatusOld = model.VehicleTroughStatus;
                            if (model.VehicleTroughStatus == null || model.VehicleTroughStatus == (int)VehicleTroughStatus.Blank || model.VehicleTroughStatus == (int)VehicleTroughStatus.Ngat_mang)
                            {
                                model.VehicleTroughStatus = (int)VehicleTroughStatus.Da_goi;
                                model.VehicleTroughTimeComeIn = DateTime.Now;
                            }

                            detail.ListTroughWeight = await _vehicleInspectionRepository.GetListTroughWeightByVehicleInspectionId(detail.Id);
                            if (detail.ListTroughWeight != null)
                            {
                                var count_TroughWeight = detail.ListTroughWeight.Where(s => s.TroughType == status).ToList();
                                if (count_TroughWeight.Count > 0)
                                {
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.ERROR,
                                        msg = "Cập nhật không thành công.Máng số " + status + " đã có"
                                    });
                                }
                            }
                            model.TimeCallVehicleTroughTimeComeIn = DateTime.Now;
                            model.TroughType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                var allcode_VehicleTrough = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                                var allcode_detail_VehicleTrough = allcode_VehicleTrough.FirstOrDefault(s => s.CodeValue == model.VehicleTroughStatus);
                                detail.VehicleTroughStatusName = allcode_detail_VehicleTrough.Description;
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.TROUGH_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.TroughType);
                                detail.TroughTypeName = allcode_detail?.Description ?? "";
                                // ✅ bắn cả máng cũ + máng mới
                                await _hubContext.Clients.All.SendAsync("UpdateMangStatus", detail.TroughType, model.TroughType, detail.Id);
                                if (model.VehicleTroughStatus == null || detail.VehicleTroughStatus == (int)VehicleTroughStatus.Blank || VehicleTroughStatusOld == (int)VehicleTroughStatus.Ngat_mang)
                                {

                                    if (detail.ListTroughWeight == null) detail.ListTroughWeight = new List<TroughWeight>();
                                    var TroughWeight_model = new TroughWeight();
                                    TroughWeight_model.TroughType = status;
                                    detail.ListTroughWeight.Add(TroughWeight_model);

                                    await _hubContext.Clients.All.SendAsync("ListCarCall", detail);

                                }
                                LogHelper.InsertLogTelegram("Xin mời xe biển số " + detail.VehicleNumber + " của tài xế " + detail.DriverName + " di chuyển vào máng số " + status + ". Trân trọng!");

                            }

                        }
                        break;
                    case 5:
                        {

                            model.VehicleTroughWeight = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            await _hubContext.Clients.All.SendAsync("ListCartoFactory", detail);
                        }
                        break;
                    case 6:
                        {
                            if (detail.VehicleTroughStatus == status && detail.VehicleTroughWeight == weight)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            if (status == (int)VehicleTroughStatus.Boc_Hang)
                            {
                                model.VehicleTroughTimeComeIn = DateTime.Now;

                            }

                            if (status == (int)VehicleTroughStatus.Hoan_thanh)
                            {
                                model.VehicleTroughTimeComeOut = DateTime.Now;
                                detail.VehicleTroughTimeComeOut = DateTime.Now;
                                if (detail.VehicleTroughStatus != (int)VehicleTroughStatus.Blank)
                                {
                                    var model_TroughWeight = new TroughWeight();
                                    model_TroughWeight.VehicleInspectionId = id;
                                    model_TroughWeight.TroughType = detail.TroughType;
                                    model_TroughWeight.VehicleTroughWeight = weight;
                                    model_TroughWeight.CreatedBy = _UserId;
                                    model_TroughWeight.StartDate = model.VehicleTroughTimeComeIn;
                                    model_TroughWeight.EndDate = DateTime.Now;
                                    _vehicleInspectionRepository.InsertTroughWeight(model_TroughWeight);
                                }



                            }

                            if (status == (int)VehicleTroughStatus.Ngat_mang)
                            {
                                model.VehicleTroughTimeComeOut = DateTime.Now;

                                var model_TroughWeight = new TroughWeight();
                                model_TroughWeight.VehicleInspectionId = id;
                                model_TroughWeight.TroughType = detail.TroughType;
                                model_TroughWeight.VehicleTroughWeight = weight;
                                model_TroughWeight.CreatedBy = _UserId;
                                model_TroughWeight.StartDate = model.VehicleTroughTimeComeIn;
                                model_TroughWeight.EndDate = DateTime.Now;
                                _vehicleInspectionRepository.InsertTroughWeight(model_TroughWeight);


                            }
                            model.VehicleTroughStatus = status;
                            model.VehicleTroughWeight = weight; // ✅ lấy từ input
                            model.Note = Note;

                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                detail.ListTroughWeight = await _vehicleInspectionRepository.GetListTroughWeightByVehicleInspectionId(detail.Id);
                                if (status == (int)VehicleTroughStatus.Ngat_mang)
                                {
                                    if (detail.ListTroughWeight == null) detail.ListTroughWeight = new List<TroughWeight>();
                                    detail.ListTroughWeight.Add(new TroughWeight());
                                }
                                if (status == (int)VehicleTroughStatus.Boc_Hang)
                                {
                                    if (detail.ListTroughWeight == null) detail.ListTroughWeight = new List<TroughWeight>();
                                    var detai_TroughWeight = new TroughWeight();
                                    detai_TroughWeight.TroughType = detail.TroughType;

                                    detail.ListTroughWeight.Add(detai_TroughWeight);
                                }
                                detail.VehicleTroughWeight = weight;
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLETROUGH_STATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleTroughStatus);
                                detail.VehicleTroughStatusName = allcode_detail.Description;
                                if (status == (int)VehicleTroughStatus.Bo_Luot)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCarCall_Bo_LUOT", detail);
                                    return Ok(new
                                    {
                                        status = (int)ResponseType.SUCCESS,
                                        msg = "cập nhật thành công"
                                    });
                                }
                                if (status == (int)VehicleTroughStatus.Hoan_thanh)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListCarCall_Da_SL", detail);
                                }
                                else
                                {
                                    if(detail.ListTroughWeight!=null)
                                     detail.ListTroughWeight.Add(new TroughWeight());
                                    await _hubContext.Clients.All.SendAsync("ListCarCall", detail);
                                }
                            }
                        }
                        break;
                    case 7:
                        {
                            if (detail.VehicleWeighingStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            if (status == (int)VehicleWeighingStatus.DA_Can_Ra)
                            {
                                model.VehicleWeighingTimeComplete = DateTime.Now;
                                detail.VehicleWeighingTimeComplete = DateTime.Now;
                            }
                            model.VehicleWeighingStatus = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHINGSTATUS);
                            var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighingStatus);
                            detail.VehicleWeighingStatusName = allcode_detail.Description;
                            detail.ListTroughWeight = await _vehicleInspectionRepository.GetListTroughWeightByVehicleInspectionId(detail.Id);
                            if (status == (int)VehicleWeighingStatus.DA_Can_Ra)
                            {
                                await _hubContext.Clients.All.SendAsync("ListVehicles_Da_SL", detail);
                            }
                            else
                            {
                                await _hubContext.Clients.All.SendAsync("ListVehicles", detail);
                            }


                        }
                        break;
                    case 8:
                        {
                            if (model.VehicleWeighingType == (int)VehicleWeighingType.DA_Vao_Can)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe Đã vào cân"
                                });
                            }
                            if (detail.LoadingStatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            if (weight > 0)
                            {
                                var update = await _vehicleInspectionRepository.UpdateVehicleLoadTaken(id, weight);
                            }
                            model.LoadingStatus = status;
                            model.UpdatedBy = _UserId;
                            model.ProcessingIsLoadingDate = DateTime.Now;
                            detail.ProcessingIsLoadingDate = DateTime.Now;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.LoadType);
                                detail.LoadTypeName = allcode_detail == null ? "" : allcode_detail.Description;
                                var allcode2 = await _allCodeRepository.GetListSortByName(AllCodeType.LOADINGSTATUS);
                                var allcode_detail2 = allcode2.FirstOrDefault(s => s.CodeValue == model.LoadingStatus);
                                detail.LoadingStatusName = allcode_detail2 == null ? "" : allcode_detail2.Description;
                                detail.VehicleLoadTaken = weight;
                                if (status == (int)LoadingStatus.Da_HTTC)
                                {
                                    await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync("ListProcessingIsLoading", detail);
                                }
                            }


                        }
                        break;
                    case 9:
                        {
                            if (model.VehicleTroughStatus != (int)VehicleTroughStatus.Blank && model.VehicleTroughStatus != null)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Xe đang được gọi vào máng"
                                });
                            }
                            if (detail.VehicleWeighedstatus == status)
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.ERROR,
                                    msg = "Cập nhật không thành công.Tình trạng xe không thay đổi"
                                });
                            }
                            model.VehicleWeighedstatus = status;
                            if (status == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                            {
                                model.VehicleWeighingTimeComeOut = DateTime.Now;
                                model.VehicleTroughStatus = (int)VehicleTroughStatus.Blank;
                                detail.VehicleWeighingTimeComeOut = DateTime.Now;
                            }
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);
                            if (detail.VehicleWeighedstatus == null && model.VehicleWeighedstatus == (int)VehicleWeighedstatus.Blank)
                            {
                                break;
                            }
                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLEWEIGHEDSTATUS);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.VehicleWeighedstatus);
                                detail.VehicleWeighedstatusName = allcode_detail == null ? "" : allcode_detail.Description;

                                if (status == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao)
                                {
                                    if (detail.ListTroughWeight == null) detail.ListTroughWeight = new List<TroughWeight>();
                                    var TroughWeight_model = new TroughWeight();
                                    TroughWeight_model.TroughType = status;
                                    detail.ListTroughWeight.Add(TroughWeight_model);
                                    await _hubContext.Clients.All.SendAsync("ListWeighedInput_Da_SL", detail);
                                }
                                else
                                {
                                    await _hubContext.Clients.All.SendAsync("ListWeighedInput", detail);
                                }
                            }


                        }
                        break;

                    case 10:
                        {

                            model.LoadingType = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.LoadingType);
                                detail.LoadingTypeName = allcode_detail == null ? "" : allcode_detail.Description;
                                // ✅ bắn cả máng cũ + máng mới

                                await _hubContext.Clients.All.SendAsync("ProcessingIsLoading_khoa", detail);

                            }
                        }
                        break;
                    case 11:
                        {

                            model.Rank = status;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                var allcode = await _allCodeRepository.GetListSortByName(AllCodeType.Rank_Type);
                                var allcode_detail = allcode.FirstOrDefault(s => s.CodeValue == model.Rank);
                                detail.RankName = allcode_detail == null ? "" : allcode_detail.Description;
                                // ✅ bắn cả máng cũ + máng mới

                                await _hubContext.Clients.All.SendAsync("ProcessingIsLoading_khoa", detail);

                            }
                        }
                        break;
                    case 12:
                        {

                            model.CSNotes = Note;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                detail.CSNotes = Note;
                                await _hubContext.Clients.All.SendAsync("ProcessingIsLoading_khoa", detail);

                            }
                        }
                        break;
                    case 13:
                        {

                            model.Note = Note;
                            UpdateCar = await _vehicleInspectionRepository.UpdateCar(model);

                            if (UpdateCar > 0)
                            {
                                detail.Note = Note;
                                await _hubContext.Clients.All.SendAsync("ListCarCall", detail);
                                return Ok(new
                                {
                                    status = (int)ResponseType.SUCCESS,
                                    msg = "cập nhật thành công"
                                });

                            }
                            else
                            {
                                return Ok(new
                                {
                                    status = (int)ResponseType.FAILED,
                                    msg = "cập nhật không thành công"
                                });

                            }
                        }
                        break;
                }
                if (UpdateCar > 0)
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OpenPopup - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        //danh sách xe đang ký
        public IActionResult RegisteredVehicle()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return View();
        }
        public async Task<IActionResult> ListRegisteredVehicle(CartoFactorySearchModel SearchModel)
        {
            try
            {
                ViewBag.type = SearchModel.type;//1 đã SL
                var AllCode = await _allCodeRepository.GetListSortByName(AllCodeType.VEHICLE_STATUS);
                var LoadingType = await _allCodeRepository.GetListSortByName(AllCodeType.Loading_Type);
                ViewBag.AllCode = AllCode;
                ViewBag.LoadingType = LoadingType;
                var data = await _vehicleInspectionRepository.GetListRegisteredVehicle(SearchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListCartoFactory - CarController: " + ex);
            }
            return PartialView();
        }
        public async Task<IActionResult> UpdateTroughWeight(int id, int VehicleTroughWeight)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var model_TroughWeight = new TroughWeight();
                model_TroughWeight.Id = id;
                model_TroughWeight.TroughType = null;
                model_TroughWeight.VehicleTroughWeight = VehicleTroughWeight;
                model_TroughWeight.StartDate = null;
                model_TroughWeight.UpdateBy = _UserId;
                var update = await _vehicleInspectionRepository.UpdateTroughWeight(model_TroughWeight);
                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        public async Task<IActionResult> CancelTroughWeight(int id)
        {
            try
            {

                var detail = await _vehicleInspectionRepository.GetDetailTroughWeightById(id);
                if (detail != null)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = detail.VehicleTroughWeight,
                        msg = "Hủy thao tác thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "Thao tác không thành công"
            });
        }
        public async Task<IActionResult> AddOrUpdateNamePopup(int id)
        {
            if (id > 0)
            {
                ViewBag.Id = id;
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                ViewBag.name = detail.CustomerName;

                return PartialView();
            }
            return PartialView();
        }
        public async Task<IActionResult> UpdateName(int id, string name = null, string VehicleNumber = null)
        {
            try
            {
                var model = new VehicleInspectionUpdateModel();
                model.Id = id;
                if (name != null)
                    model.CustomerName = name;
                if (VehicleNumber != null)
                {
                    model.VehicleNumber = VehicleNumber;
                    var audio = await _vehicleInspectionRepository.GetAudioPathByVehicleNumber(VehicleNumber);
                    model.AudioPath = audio;
                }

                var Update = await _vehicleInspectionRepository.UpdateCar(model);
                if (Update > 0)
                {
                    if (VehicleNumber != null && (model.AudioPath == null || model.AudioPath == ""))
                    {
                        var request = new RegistrationRecord();
                        request.Id = id;
                        request.Bookingid = id;
                        request.Type = 2;
                        request.PlateNumber = VehicleNumber;
                        request.text_voice = "Mời biển số xe " + request.PlateNumber + " vào cân";
                        var Queue = _workQueueClient.SyncQueue(request);
                        if (!Queue)
                        {
                            Queue = _workQueueClient.SyncQueue(request);
                        }
                    }

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "Cập nhật  thành công"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Cập nhật không thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Cập nhật không thành công"
            });
        }
        public async Task<IActionResult> UpdateVehicleLoadTaken(int id, int vehicleloadtaken)
        {
            try
            {

                var update = await _vehicleInspectionRepository.UpdateVehicleLoadTaken(id, vehicleloadtaken);
                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        public async Task<IActionResult> UpdateRegisteredVehicle(int id, int status, string note = null)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var UpdateCar = 0;
                ViewBag.Id = id;
                ViewBag.StatusCar = 0;
                var model = new VehicleInspectionUpdateModel();
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                model.Id = detail.Id;
                model.RecordNumber = detail.RecordNumber;
                model.CustomerName = detail.CustomerName;
                model.VehicleNumber = detail.VehicleNumber;
                model.RegisterDateOnline = detail.RegisterDateOnline;
                model.DriverName = detail.DriverName;
                model.LicenseNumber = detail.LicenseNumber;
                model.PhoneNumber = detail.PhoneNumber;
                model.VehicleLoad = detail.VehicleLoad;
                model.VehicleStatus = detail.VehicleStatus;
                model.LoadType = detail.LoadType;
                model.IssueCreateDate = detail.IssueCreateDate;
                model.IssueUpdatedDate = detail.IssueUpdatedDate;
                model.VehicleWeighingType = detail.VehicleWeighingType;
                model.VehicleWeighingTimeComeIn = detail.VehicleWeighingTimeComeIn;
                model.VehicleWeighingTimeComeOut = detail.VehicleWeighingTimeComeOut;
                model.VehicleWeighingTimeComplete = detail.VehicleWeighingTimeComplete;
                model.TroughType = detail.TroughType;
                model.VehicleTroughTimeComeIn = detail.VehicleTroughTimeComeIn;
                model.VehicleTroughTimeComeOut = detail.VehicleTroughTimeComeOut;
                model.VehicleTroughWeight = detail.VehicleTroughWeight;
                model.VehicleTroughStatus = detail.VehicleTroughStatus;
                model.LoadingStatus = detail.LoadingStatus;
                model.VehicleWeighedstatus = detail.VehicleWeighedstatus;
                model.TimeCallVehicleTroughTimeComeIn = detail.TimeCallVehicleTroughTimeComeIn;
                model.LoadingType = detail.LoadingType;
                model.VehicleArrivalDate = detail.VehicleArrivalDate;
                model.ProcessingIsLoadingDate = detail.ProcessingIsLoadingDate;
                model.VehicleWeightMax = detail.VehicleWeightMax;
                model.VehicleLoadTaken = detail.VehicleLoadTaken;
                model.CreatedBy = _UserId;
                model.VehicleStatus = status;
                model.VehicleArrivalDate = DateTime.Now;
                model.ProtectNotes = note;
                model.TrangThai = detail.TrangThai;
                model.Rank = detail.Rank;
                model.RankName = detail.RankName;
                model.CSNotes = detail.CSNotes;
                model.VehicleWeightIn = detail.VehicleWeightIn;
                model.VehicleWeightOut = detail.VehicleWeightOut;
                var update = await _vehicleInspectionRepository.UpdateCar(model);
                if (status == (int)VehicleStatus.Da_Den_NM)
                {
                    await _hubContext.Clients.All.SendAsync("ListCartoFactory_Da_SL", detail);
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("ListCartoFactory", detail);
                }
                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        public async Task<IActionResult> AddOrUpdateVehicleNumber(int id)
        {
            if (id > 0)
            {
                ViewBag.Id = id;
                var detail = await _vehicleInspectionRepository.GetDetailtVehicleInspection(id);
                ViewBag.vehicleNumber = detail.VehicleNumber;

                return PartialView();
            }
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var SearchModel = new CartoFactorySearchModel();
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                string _FileName = StringHelpers.GenFileName("Danh sách đơn hàng", _UserId, "xlsx");
                string _UploadFolder = @"Template\Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }
                //delete all file in folder before export
                try
                {
                    System.IO.DirectoryInfo di = new DirectoryInfo(_UploadDirectory);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }
                catch
                {
                }
                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                var rsPath = await _vehicleInspectionRepository.ExportDeposit(SearchModel, FilePath);

                if (!string.IsNullOrEmpty(rsPath))
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xuất dữ liệu thành công",
                        path = "/" + _UploadFolder + "/" + _FileName
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xuất dữ liệu thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportExcel - OrderController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        public async Task<IActionResult> UpdateVehicleWeightIn(int id, int VehicleWeightIn)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var model = new VehicleInspectionUpdateModel();
                model.Id = id;
                model.VehicleWeightIn = VehicleWeightIn;
                model.UpdatedBy = _UserId;

                var update = await _vehicleInspectionRepository.UpdateCar(model);
                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
        public async Task<IActionResult> UpdateVehicleWeightOut(int id, int VehicleWeightOut)
        {
            try
            {
                var _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                var model = new VehicleInspectionUpdateModel();
                model.Id = id;
                model.VehicleWeightOut = VehicleWeightOut;
                model.UpdatedBy = _UserId;

                var update = await _vehicleInspectionRepository.UpdateCar(model);
                if (update > 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = "cập nhật thành công"
                    });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("WeighedInput - CarController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.ERROR,
                msg = "cập nhật không thành công"
            });
        }
    }
}
