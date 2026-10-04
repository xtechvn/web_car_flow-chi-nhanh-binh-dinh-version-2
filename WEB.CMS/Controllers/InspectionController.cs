using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class InspectionController : Controller
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IConfiguration _configuration;

        public InspectionController(IConfiguration configuration, IInspectionRepository inspectionRepository)
        {
            _configuration = configuration;
            _inspectionRepository = inspectionRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetListInspection(InspectionSearchModel searchModel)
        {
            try
            {
                var data = await _inspectionRepository.GetListInspection(searchModel);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInspection - InspectionController: " + ex);
            }

            return PartialView();
        }
        public async Task<IActionResult> AddOrUpdate(int id)
        {
            if (id > 0)
            {
                ViewBag.Id = id;
                var detail = await _inspectionRepository.GetDetailInspectionById(id);
                return PartialView(detail);
            }
            return PartialView();
        }
        public async Task<IActionResult> SetUp(Inspection model)
        {
            try
            {
                if (model != null && model.Id == 0)
                {
                    var checkVehicleNumber = await _inspectionRepository.CheckVehicleNumber(model.VehicleNumber);
                    if (checkVehicleNumber != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Biển số xe đã tồn tại, Vui lòng kiểm tra lại"
                        });
                    }
                    var Insert = await _inspectionRepository.InsertInspection(model);
                    if (Insert > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Thêm mới thành công"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Thêm mới không thành công"
                        });
                    }


                }
                else
                {
                    var update = await _inspectionRepository.UpdateInspection(model);
                    if (update > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Cập nhật thành công"
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

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUp - InspectionController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Đã có lỗi xảy ra, Vui lòng liên hệ IT"
            });
        }
    }
}
