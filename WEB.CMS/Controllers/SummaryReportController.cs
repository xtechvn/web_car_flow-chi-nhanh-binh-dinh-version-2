using Entities.ViewModels.Car;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class SummaryReportController : Controller
    {
        private readonly IVehicleInspectionRepository _vehicleInspectionRepository;
        private readonly IAllCodeRepository _allCodeRepository;

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        public SummaryReportController(IVehicleInspectionRepository vehicleInspectionRepository, IConfiguration configuration,
            IAllCodeRepository allCodeRepository, IWebHostEnvironment WebHostEnvironment)
        {
            _vehicleInspectionRepository = vehicleInspectionRepository;
            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
            _WebHostEnvironment = WebHostEnvironment;
        }
        public async Task<IActionResult> Index()
        { 
            return View();
        }  
        public async Task<IActionResult> TotalVehicleInspection( string date)
        {
            var date_time = date != null && date != "" ? DateUtil.StringToDate(date) : null;

            var Total = await _vehicleInspectionRepository.CountTotalVehicleInspectionSynthetic(date_time, date_time);
            ViewBag.TotalData = Total;
            return View();
        }
        public async Task<IActionResult> DetailSummaryReport()
        {
            var LOAD_TYPE = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
            ViewBag.LOAD_TYPE = LOAD_TYPE;
            return View();
        }
        public async Task<IActionResult> DailyStatistics(SummaryReportSearchModel SearchModel)
        {
            try
            {
                var FromDate = SearchModel.FromDate != null && SearchModel.FromDate != "" ? DateUtil.StringToDate(SearchModel.FromDate) : null;
                var ToDate = SearchModel.ToDate != null && SearchModel.ToDate != "" ? DateUtil.StringToDate(SearchModel.ToDate) : null;

                var data = await _vehicleInspectionRepository.GetListVehicleInspectionSynthetic(FromDate, ToDate, SearchModel.LoadType);
                var Total = await _vehicleInspectionRepository.CountTotalVehicleInspectionSynthetic(FromDate, ToDate);
                ViewBag.TotalData = Total;
                
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DailyStatistics - SummaryReportController: " + ex);
            }
            return PartialView();
        }   
        public async Task<IActionResult> TotalWeightByTroughType(string date)
        {
            try
            {
                var date_time = date!=null && date !=""? DateUtil.StringToDate(date):null;
                var data = await _vehicleInspectionRepository.GetTotalWeightByTroughType(date_time);
                var datamodel = new TotalWeightByHourViewModel();
               
                datamodel.TroughType = data.Select(x => x.TroughType).ToArray();
                datamodel.SanLuong = data.Select(x => x.SanLuong).ToArray();
                datamodel.SoXe = data.Select(x => x.SoXe).ToArray();
                datamodel.TongGio = data.Select(x => x.TongGio).ToArray();
                datamodel.Tan_Moi_Gio = data.Select(x => x.Tan_Moi_Gio).ToArray();

                return Ok(new
                {
                    isSuccess = true,
                    data = datamodel
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByWeightGroup - SummaryReportController: " + ex);
            }
            return Ok(new
            {
                isSuccess = false,

            });
        }
        public async Task<IActionResult> GetTotalWeightByWeightGroup(string date)
        {
            try
            {
                var date_time = date!=null && date !=""? DateUtil.StringToDate(date):null;
            
                var data = await _vehicleInspectionRepository.GetTotalWeightByWeightGroup(date_time);
                var datamodel=new TotalWeightByHourViewModel();
                datamodel.CompletionHour = data.Select(x => x.CompletionHour).ToArray();
                datamodel.TotalWeightInHour = data.Select(x => x.TotalWeightInHour).ToArray();
                datamodel.WeightGroup = data.Select(x => x.WeightGroup).ToArray();
                datamodel.TotalWeightTons = data.Select(x => x.TotalWeightTons).ToArray();
                datamodel.TotalProcessMinutes = data.Select(x => x.TotalProcessMinutes).ToArray();
                datamodel.SanLuong = data.Select(x => x.SanLuong).ToArray();
                datamodel.SoPhut_Tren_Tan = data.Select(x => x.SoPhut_Tren_Tan).ToArray();
                datamodel.SoPhut_Tren_Xe = data.Select(x => x.SoPhut_Tren_Xe).ToArray();

                return Ok(new
                {
                    isSuccess = true,
                    data = datamodel
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByWeightGroup - SummaryReportController: " + ex);
            }
            return Ok(new
            {
                isSuccess = false,
               
            });
        }
        public async Task<IActionResult> GetTotalWeightByHour(string date)
        {
            try
            {
                var date_time = date != null && date != "" ? DateUtil.StringToDate(date) : null;

                var data = await _vehicleInspectionRepository.GetTotalWeightByHour(date_time);
                var datamodel = new TotalWeightByHourViewModel();
                datamodel.KhungGio = data.Select(x => x.KhungGio).ToArray();
                datamodel.SanLuong = data.Select(x => x.SanLuong).ToArray();
                datamodel.Tan_Moi_Gio = data.Select(x => x.Tan_Moi_Gio).ToArray();
            

                return Ok(new
                {
                    isSuccess = true,
                    data = datamodel
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByWeightGroup - SummaryReportController: " + ex);
            }
            return Ok(new
            {
                isSuccess = false,

            });
        }
        public async Task<IActionResult> GetProductivityStatistics(string date)
        {
            try
            {
                var date_time = date != null && date != "" ? DateUtil.StringToDate(date) : null;
                var data = await _vehicleInspectionRepository.CountTotalVehicleInspectionSynthetic(date_time, date_time);
             
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetProductivityStatistics - SummaryReportController: " + ex);
            }
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> ExportExcel(SummaryReportSearchModel SearchModel)
        {
            try
            {
                var FromDate = SearchModel.FromDate != null && SearchModel.FromDate != "" ? DateUtil.StringToDate(SearchModel.FromDate) : null;
                var ToDate = SearchModel.ToDate != null && SearchModel.ToDate != "" ? DateUtil.StringToDate(SearchModel.ToDate) : null;

                var data = await _vehicleInspectionRepository.GetListVehicleInspectionSynthetic(FromDate, ToDate, SearchModel.LoadType);

               
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

                var rsPath = await _vehicleInspectionRepository.ExportSummaryReport(data, FilePath);

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
        [HttpPost]
        public async Task<IActionResult> ExportExe(SummaryReportSearchModel SearchModel)
        {
            try
            {
                var FromDate = SearchModel.FromDate != null && SearchModel.FromDate != "" ? DateUtil.StringToDate(SearchModel.FromDate) : null;
                var ToDate   = SearchModel.ToDate != null && SearchModel.ToDate != "" ? DateUtil.StringToDate(SearchModel.ToDate) : null;

                var data = await _vehicleInspectionRepository.GetListVehicleInspectionSynthetic(FromDate, ToDate, SearchModel.LoadType);

                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                // Tên file exe, ví dụ: Report_123_20240428.exe
                string _FileName = StringHelpers.GenFileName("Report", _UserId, "exe");
                string _UploadFolder = @"Template\\Export";
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                if (!Directory.Exists(_UploadDirectory))
                {
                    Directory.CreateDirectory(_UploadDirectory);
                }

                // Xóa các file .exe cũ trong thư mục
                try
                {
                    foreach (var file in new DirectoryInfo(_UploadDirectory).GetFiles("*.exe"))
                    {
                        file.Delete();
                    }
                }
                catch { }

                string FilePath = Path.Combine(_UploadDirectory, _FileName);

                // Tạo một file exe giả (placeholder). Thực tế bạn có thể sao chép một exe mẫu ở đây.
                // Đối với demo, ghi một vài byte để tạo file hợp lệ.
                byte[] dummyExe = new byte[] { 0x4D, 0x5A }; // "MZ" header of Windows exe
                await System.IO.File.WriteAllBytesAsync(FilePath, dummyExe);

                return new JsonResult(new
                {
                    isSuccess = true,
                    message   = "Xuất file exe thành công",
                    path      = "/" + _UploadFolder + "/" + _FileName
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportExe - SummaryReportController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message   = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> TimeProductDelivery()
        {
            var LOAD_TYPE = await _allCodeRepository.GetListSortByName(AllCodeType.LOAD_TYPE);
            ViewBag.LOAD_TYPE = LOAD_TYPE;
            return View();
        }
        public async Task<IActionResult> GetlistTimeProductDelivery(SummaryReportSearchModel SearchModel)
        {
            try
            {
                var FromDate = SearchModel.FromDate != null && SearchModel.FromDate != "" ? DateUtil.StringToDate(SearchModel.FromDate) : null;
                var ToDate = SearchModel.ToDate != null && SearchModel.ToDate != "" ? DateUtil.StringToDate(SearchModel.ToDate) : null;

                var data = await _vehicleInspectionRepository.GetListVehicleInspectionTimeProductDelivery(FromDate, ToDate, SearchModel.LoadType);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetlistTimeProductDelivery - SummaryReportController: " + ex);
            }
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> ExportTimeProductDelivery(SummaryReportSearchModel SearchModel)
        {
            try
            {
                var FromDate = SearchModel.FromDate != null && SearchModel.FromDate != "" ? DateUtil.StringToDate(SearchModel.FromDate) : null;
                var ToDate = SearchModel.ToDate != null && SearchModel.ToDate != "" ? DateUtil.StringToDate(SearchModel.ToDate) : null;

                var data = await _vehicleInspectionRepository.GetListVehicleInspectionTimeProductDelivery(FromDate, ToDate, SearchModel.LoadType);


                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                string _FileName = StringHelpers.GenFileName("Danh sách", _UserId, "xlsx");
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

                var rsPath = await _vehicleInspectionRepository.ExportTimeProductDelivery(data, FilePath);

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
    }
}

