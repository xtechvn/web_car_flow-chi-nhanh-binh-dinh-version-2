using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ManagementUser _ManagementUser;
        private IMenuRepository _MenuRepository;
        public HomeController(ILogger<HomeController> logger, ManagementUser managementUser, IMenuRepository menuRepository)
        {
            _logger = logger;
            _ManagementUser = managementUser;
            _MenuRepository = menuRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                IEnumerable<int> menu_ids = new List<int>();
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null && current_user.Permissions != null && current_user.Permissions.Any())
                {
                    menu_ids = current_user.Permissions.Where(s => s.PermissionId == (int)PermissionType.TRUY_CAP ).Select(s => s.MenuId).ToList();

                }
                var menus = await _MenuRepository.GetMenuPermissionOfUser(menu_ids);
                var path = (string)HttpContext.Request.Path;
                var actived = menus.FirstOrDefault(s => path.ToLower() == (s.Link ?? string.Empty).ToLower());
                var menu_id = 115;
                var parent_id = GetRootParentId(menus, menu_id);
                ViewBag.MenuId = menu_id;
                ViewBag.ParentId = parent_id;
                ViewBag.Menu = menus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                LogHelper.InsertLogTelegram("Index - HomeController: " + ex);
            }
            return View();
        }

        public IActionResult DataMonitor()
        {
            return RedirectToAction("Index", "Error");
        }

        public IActionResult ExecuteQuery(string dataQuery)
        {

            return RedirectToAction("Index", "Error");
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Error()
        {
            ViewBag.UserName = "";
            return View();
        }
        private int GetRootParentId(IEnumerable<Menu> datas, int child_id)
        {
            var model = datas.FirstOrDefault(s => s.Id == child_id);
            if (model != null)
            {
                if (model.ParentId <= 0)
                {
                    return model.Id;
                }
                else
                {
                    return GetRootParentId(datas, model.ParentId.Value);
                }
            }
            else
            {
                return -1;
            }
        }
        public IActionResult UserChangePass()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                var model = new UserDataViewModel()
                {
                    Id = current_user.Id
                };
                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UserChangePass - UserController: " + ex);
                return Content("");
            }

        } 
        public IActionResult SafetyNotice()
        {
            return View();
        }

    }
    
}
