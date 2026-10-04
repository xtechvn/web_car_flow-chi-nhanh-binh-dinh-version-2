using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Repositories.IRepositories;
using StackExchange.Redis;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Services;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _UserRepository;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IRoleRepository _RoleRepository;
        private readonly IDepartmentRepository _DepartmentRepository;
        private ManagementUser _ManagementUser;
        private RedisConn _redisConn;
        public UserController(IUserRepository userRepository, ManagementUser managementUser, IConfiguration configuration, IWebHostEnvironment hostEnvironment, IRoleRepository roleRepository, IDepartmentRepository departmentRepository)
        {
            _UserRepository = userRepository;
            _configuration = configuration;
            _WebHostEnvironment = hostEnvironment;
            _RoleRepository = roleRepository;
            _ManagementUser = managementUser;
            _DepartmentRepository = departmentRepository;
            _redisConn = new RedisConn(configuration);
            _redisConn.Connect();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(string userName, int? status, int currentPage = 1, int pageSize = 20)
        {
            var model = new List<UserGridModel>();
            try
            {
                if (status < 0 || status > 2) status = null;
                if (userName != null && userName.Trim() != "") userName = CommonHelper.RemoveUnicode(userName);
                model = _UserRepository.GetPagingList(userName, status, currentPage, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - UserController: " + ex);
            }
            return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpSert(IFormFile imagefile, UserViewModel model)
        {
            try
            {
                string imageUrl = string.Empty;
                if (imagefile != null)
                {
                    string _FileName = Guid.NewGuid() + Path.GetExtension(imagefile.FileName);
                    string _UploadFolder = @"uploads/images";
                    string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, _UploadFolder);

                    if (!Directory.Exists(_UploadDirectory))
                    {
                        Directory.CreateDirectory(_UploadDirectory);
                    }

                    string filePath = Path.Combine(_UploadDirectory, _FileName);

                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagefile.CopyToAsync(fileStream);
                        }
                    }
                    model.Avata = "/" + _UploadFolder + "/" + _FileName;
                }

                int rs = 0;
                if (model.UserPositionId != null && model.UserPositionId > 0)
                {
                    var active_position = await _UserRepository.GetUserPositionsByID((int)model.UserPositionId);
                    if (active_position != null) model.Level = active_position.Rank;
                }
                if (model.CompanyType == null || model.CompanyType.Trim() == "")
                {
                    model.CompanyType = _configuration["CompanyType"];
                }
                var _UserLogin = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                model.CreatedBy = _UserLogin;
                model.ModifiedBy = _UserLogin;
                if (model.Phone == null) model.Phone = "";
                if (model.Avata == null) model.Avata = "";
                if (model.Address == null) model.Address = "";
                if (model.NickName == null) model.NickName = "";

                //-- Update dbUser:

                var exists = await _UserRepository.GetById(model.Id);
                if (exists == null || exists.Id <= 0)
                {
                    rs = await _UserRepository.Create(model);

                }
                else
                {
                    rs = await _UserRepository.Update(model);

                }

                if (rs > 0)
                {
                    _redisConn.clear(CacheName.USER_ROLE + model.Id + "_" + _configuration["CompanyType"], Convert.ToInt32(_configuration["Redis:Database:db_common"]));
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Tên đăng nhập hoặc email đã tồn tại"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpSert - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetDetail(int Id)
        {
            var model = new UserDataViewModel();
            try
            {
                model = await _UserRepository.GetUser(Id);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                model.UserPositionName = model.UserPositionId != null && model.UserPositionId > 0 ? _UserRepository.GetUserPositionsByID((int)model.UserPositionId).Result.Name : "";
                ViewBag.IsMFAEnabled = Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - UserController: " + ex);
                ViewBag.IsMFAEnabled = false;
            }
            return PartialView(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserPasswordModel model)
        {
            try
            {
                var rs = await _UserRepository.ChangePassword(model);
                //var current_user = await _UserRepository.GetById(model.Id);
                //var user = await _aPIService.GetByUserDetail(current_user.Id, current_user.UserName, current_user.Email);
                //var NewPassword = EncodeHelpers.MD5Hash(model.NewPassword);
                //var result_2 = _aPIService.ChangePassword(current_user.UserName, NewPassword);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        result = rs
                    });
                }
                else if (rs == -1)
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Mật khẩu hiện tại không chính xác",
                        result = rs
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangePassword - UserController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        public async Task<IActionResult> UserProfile()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                var model = await _UserRepository.GetUser(current_user.Id);
                ViewBag.RoleList = await _RoleRepository.GetRoleListByUserId(current_user.Id);
                ViewBag.UserPosition = _UserRepository.GetUserPositions();
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserProfile - UserController: " + ex);
                return Content("");
            }

        }

        public async Task<string> GetUserSuggestionList(string name)
        {
            try
            {
                var userlist = await _UserRepository.GetUserSuggesstion(name);
                var suggestionlist = userlist.Select(s => new
                {
                    id = s.Id,
                    name = s.UserName,
                    fullname = s.FullName,
                    email = s.Email
                }).ToList();
                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggestionList - UserController: " + ex);
                return null;
            }
        }
        public async Task<IActionResult> AddOrUpdate(int Id, bool IsClone = false)
        {
            try
            {
                var model = new User();
                ViewBag.UserRoleList = null;
                ViewBag.CompanyType = "";
                if (Id != 0)
                {

                    model = await _UserRepository.FindById(Id);
                    if (IsClone)
                    {
                        model = new User
                        {
                            FullName = model.FullName,
                            UserName = model.UserName,
                            Email = model.Email,
                            Address = model.Address,
                            BirthDay = model.BirthDay,
                            Gender = model.Gender,
                            Status = model.Status,
                            Note = model.Note,
                            DepartmentId = model.DepartmentId,
                            Phone = model.Phone,
                        };
                    }
                    var list_role_active = await _UserRepository.GetUserActiveRoleList(model.Id);
                    if (list_role_active != null && list_role_active.Count > 0)
                    {
                        ViewBag.UserRoleList = list_role_active.Select(x => x.Id).ToList();
                    }

                }
                else
                {
                    model.Gender = 1;
                }
               var User_Role=await _UserRepository.GetDetailUser(Id);
                ViewBag.DepartmentList = await _DepartmentRepository.GetAll(String.Empty);
                ViewBag.RoleList = await _RoleRepository.GetAll();
                ViewBag.UserPosition = _UserRepository.GetUserPositions();
                ViewBag.RoleId = User_Role!=null && User_Role.RoleIdList.Count > 0 ? User_Role.RoleIdList[0]:0;
                return View(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - UserController: " + ex);
                return Content("");
            }

        }
        
    }
}
