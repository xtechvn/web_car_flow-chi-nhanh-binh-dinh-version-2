using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.ViewComponents
{
    public class TopBarViewComponent : ViewComponent
    {
        private readonly IMenuRepository _MenuRepository;
        private ManagementUser _ManagementUser;
        public TopBarViewComponent(IMenuRepository menuRepository, ManagementUser managementUser)
        {
            _MenuRepository = menuRepository;
            _ManagementUser = managementUser;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var _UserName = string.Empty;
            var _UserId = string.Empty;
            try
            {

                if (HttpContext.User.FindFirst(ClaimTypes.Name) != null)
                {
                    _UserName = HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                    _UserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                }

                IEnumerable<int> menu_ids = new List<int>();
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null && current_user.Permissions != null && current_user.Permissions.Any())
                {
                    menu_ids = current_user.Permissions.Where(s => s.PermissionId == (int)PermissionType.TRUY_CAP).Select(s => s.MenuId);

                }

                var menus = await _MenuRepository.GetMenuPermissionOfUser(menu_ids);
                var path = (string)HttpContext.Request.Path;
                var actived = menus.FirstOrDefault(s => path.ToLower() == (s.Link ?? string.Empty).ToLower());
                var menu_id = 122;
                var parent_id = GetRootParentId(menus, menu_id);

                ViewBag.MenuId = menu_id;
                ViewBag.ParentId = parent_id;
                ViewBag.Menu = menus;

            }
            catch
            {

            }

            ViewBag.UserId = _UserId;
            ViewBag.UserName = _UserName;
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
    }
}
    
