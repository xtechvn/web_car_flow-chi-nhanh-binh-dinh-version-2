using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _RoleRepository;
        private readonly IMenuRepository _MenuRepository;

        public RoleController(IRoleRepository roleRepository, IMenuRepository menuRepository)
        {
            _RoleRepository = roleRepository;
            _MenuRepository = menuRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddOrUpdate(int Id)
        {
            var model = new RoleViewModel();
            if (Id != 0)
            {
                var roleEntity = await _RoleRepository.GetById(Id);
                model = new RoleViewModel()
                {
                    Id = roleEntity.Id,
                    Name = roleEntity.Name,
                    Description = roleEntity.Description,
                    Status = roleEntity.Status
                };
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> UpSert(RoleViewModel model)
        {
            try
            {
                var rs = await _RoleRepository.Upsert(model);
                if (rs > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
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
                LogHelper.InsertLogTelegram("UpSert - RoleController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = "Cập nhật thất bại"
                });
            }
        }
        public async Task<string> GetRoleSuggestionList(string name)
        {
            try
            {
                var rolelist = await _RoleRepository.GetAll();

                if (!string.IsNullOrEmpty(name))
                {
                    rolelist = rolelist.Where(s => StringHelpers.ConvertStringToNoSymbol(s.Name.Trim().ToLower())
                                                   .Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower())))
                                                   .ToList();
                }
                var suggestionlist = rolelist.Take(5).Select(s => new
                {
                    id = s.Id,
                    name = s.Name
                }).ToList();

                return JsonConvert.SerializeObject(suggestionlist);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRoleSuggestionList - RoleController: " + ex);
                return null;
            }
        }

        [HttpPost]
        public IActionResult Search(string roleName, string strUserId, int currentPage = 1, int pageSize = 8)
        {
            var model = new List<RoleDataModel>();
            try
            {
                model = _RoleRepository.GetPagingList(roleName, strUserId, currentPage, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - RoleController: " + ex);
            }
            return PartialView(model);
        }
        public async Task<IActionResult> RolePermission(int Id)
        {
            var memuList = await _MenuRepository.GetAll(String.Empty, String.Empty);
            var permissionList = await _MenuRepository.GetPermissionList();
            var rolePermission = await _RoleRepository.GetRolePermissionById(Id);

            ViewBag.MenuList = memuList;
            ViewBag.MenuPermission = await _MenuRepository.GetAllMenuHasPermission();
            ViewBag.RoleId = Id;
            ViewBag.PermissionList = permissionList;

            return View(rolePermission);
        }
        public async Task<IActionResult> UpdateRolePermission(string data, int type)
        {
            try
            {
                var rs = await _RoleRepository.AddOrDeleteRolePermission(data, type);
                if (rs)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công"
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
                LogHelper.InsertLogTelegram("UpdateRolePermission - RoleController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        public async Task<IActionResult> GetDetail(int Id, int tabActive = 1)
        {
            var model = new Role();
            var userRoleModel = new RoleUserViewModel();
            try
            {
                model = await _RoleRepository.GetById(Id);
                if (tabActive == 2)
                {
                    userRoleModel.RoleId = Id;
                    userRoleModel.ListUser = await _RoleRepository.GetListUserOfRole(Id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - RoleController: " + ex);
            }
            ViewBag.TabActive = tabActive;
            ViewBag.ListUserInRole = userRoleModel;
            return View(model);
        }

        public async Task<IActionResult> RoleListUser(int Id)
        {
            var model = new RoleUserViewModel();
            try
            {
                model.RoleId = Id;
                model.ListUser = await _RoleRepository.GetListUserOfRole(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("RoleListUser - RoleController: " + ex);
            }
            return View(model);
        }
    }
}
