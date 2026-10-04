using Entities.ConfigModels;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System.Security.Claims;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _UserRepository;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public AccountController(IUserRepository userRepository,  IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _UserRepository = userRepository;
            _configuration = configuration;
            _WebHostEnvironment = hostEnvironment;

        }
        /// <summary>
        /// Function Đăng xuất
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            try
            {
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
              
            }
            catch { }
            return Redirect(ReadFile.LoadConfig().LoginURL);

        }
        public IActionResult RedirectLogin()
        {
            try
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            }
            catch { }
            return RedirectToAction("Login", "Account");
        }
        /// <summary>
        /// Login with token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("login/token/{token_user}")]
        public async Task<IActionResult> LoginWithToken(string token_user)
        {
            try
            {
                JArray objParr = null;
                #region Test:

                //var input_test = new
                //{
                //    user_id = 2139,
                //    user_name = "minh.nq",
                //    email = "minhnguyen@usexpress.vn",
                //    time = DateTime.Now
                //};
                //var data_product = JsonConvert.SerializeObject(input_test);
                //token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:api_manual"]);
                //token = token.Replace("//", "_").Replace("+", "-");


                #endregion
                if (CommonHelper.GetParamWithKey(token_user.Replace("_", @"/").Replace("-", "+"), out objParr, _configuration["DataBaseConfig:key_api:api_manual"]))
                {
                    //LogHelper.InsertLogTelegram("CMS Login : login/token/" + token_user + "\n Data:\n " + JsonConvert.SerializeObject(objParr));
                    int _UserId = 0;
                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }
                    int user_id = Convert.ToInt32(objParr[0]["user_id"].ToString());
                    DateTime? token_time  = Convert.ToDateTime(objParr[0]["time"].ToString());
                    string user_name = objParr[0]["user_name"].ToString();
                    string email = objParr[0]["email"].ToString();
                    int token_exprire = ReadFile.LoadConfig().LoginTokenExprire;

                    if (user_id <= 0 || token_time == null || ((DateTime)token_time).AddMinutes(token_exprire) < DateTime.Now)
                    {

                    }
                    else
                    {
                        var user_exists = await _UserRepository.GetById(user_id);
                        var model = await _UserRepository.GetDetailUser(user_id);
                        if (model.Entity == null) model.Entity = new Entities.Models.User();
                        model.Entity.Id = user_id;
                        model.Entity.UserName = user_name;
                        model.Entity.Email = email;
                        await CreateCookieAuthenticate(model);
                        return RedirectToAction("Index", "Home");
                       
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CMS Login : login/token/" + token_user + "\nError: " + ex);
            }
            if (_configuration["Config:On_QC_Environment"] != null && _configuration["Config:On_QC_Environment"].Trim() == "1")
            {
                return RedirectToAction("Login", "Account");
            }
            return Redirect(ReadFile.LoadConfig().LoginURL);

        }
        /// <summary>
        /// Login with token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("login/check-login")]
        public async Task<IActionResult> CheckIfUserLogged([FromForm]string token)
        {
            try
            {
                JArray objParr = null;
                #region Test:

                //var input_test = new
                //{
                //    user_id = 18,

                //};
                //var data_product = JsonConvert.SerializeObject(input_test);
                //token = CommonHelper.Encode(data_product, _configuration["DataBaseConfig:key_api:api_manual"]);
                //token = token.Replace("//", "_").Replace("+", "-");


                #endregion
                token = token.Replace("_", "//").Replace("-", "+");
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["DataBaseConfig:key_api:api_manual"]))
                {
                    int user_id = user_id = Convert.ToInt32(objParr[0]["user_id"].ToString());
                    int _UserId = 0;

                    if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                    {
                        _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    }
                    if (_UserId>0 && _UserId == user_id)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            Logged = 1
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            Logged = 0
                        });
                    }
                }
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckIfUserLogged - AccountController" + ex);

            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                Logged = -1
            });
        }
        private async Task CreateCookieAuthenticate(UserDetailViewModel model)
        {
            try
            {
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Entity.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Name, model.Entity.UserName));
                claims.Add(new Claim("DepartmentId", (model.Entity.DepartmentId ?? 0).ToString()));
                claims.Add(new Claim(ClaimTypes.Email, model.Entity.Email));
                claims.Add(new Claim(ClaimTypes.Role, string.Join(",", model.RoleIdList)));

                //--Get and Cache Permission:
                UserRoleCacheModel user_role_cache = new UserRoleCacheModel();
                var role_permission = await _UserRepository.GetUserPermissionById(model.Entity.Id);
                if (role_permission != null && role_permission.Any())
                {
                    user_role_cache.Permission = role_permission.Select(s => new PermissionData
                    {
                        MenuId = s.MenuId,
                        RoleId = s.RoleId,
                        PermissionId = s.PermissionId
                    });
                }
                else
                {
                    user_role_cache.Permission = Enumerable.Empty<PermissionData>();
                }
                user_role_cache.UserUnderList = _UserRepository.GetListUserByUserId(model.Entity.Id);
                
                //string token = data_encode;
                
                //-- Login:
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            }
            catch
            {
                throw;
            }
        }

        public IActionResult Login(string url = "/")
        {
          
                ViewBag.ReturnURL = url;
                return View();
        }

        /// <summary>
        /// Function thực hiện xử lý đăng nhập bước 1.
        /// </summary>
        /// <param name="entity"> Thông tin đăng nhập</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmLogin(AccountModel model)
        {
            try
            {
                //-- Validate Input
                if (model == null || model.UserName == null || model.UserName.Trim() == "" || model.Password == null || model.Password.Trim() == "")
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản / Mật khẩu không được để trống, vui lòng thử lại"
                    });
                }
                //-- Bỏ ký tự đặc biệt
                model.ReturnUrl = CommonHelper.RemoveAllSpecialCharacterinURL(model.ReturnUrl);
                model.UserName = CommonHelper.RemoveAllSpecialCharacterLogin(model.UserName);
                model.UserName = model.UserName.Replace("+", "").Replace("//", "").Replace("=", "");
                model.Password = CommonHelper.RemoveAllSpecialCharacterLogin(model.Password);
                //-- Kiểm tra user/pass
                var user = await _UserRepository.CheckExistAccount(model);
                if (user == null || user.Entity == null || user.Entity.Id <= 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản / Mật khẩu không chính xác, vui lòng thử lại"
                    });
                }
                //-- Nếu tài khoản bị khóa
                if (user.Entity.Status != 0)
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ IT"
                    });
                }
                //-- Nếu môi trường QC
                await CreateCookieAuthenticate(user);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Đăng nhập thành công",
                    direct = model.ReturnUrl ?? "/"
                });


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ConfirmLogin - AccountController" + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Có lỗi xảy ra trong quá trình đăng nhập, vui lòng liên hệ IT"
            });
        }
       
    }
}
