using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserDAL _UserDAL;
        private readonly UserPositionDAL _userPositionDAL; 
        private readonly IHttpContextAccessor _HttpContext;
        private readonly UserRoleDAL _userRoleDAL;
        private readonly UserDepartDAL _userDepart;
        private readonly DepartmentDAL _departmentDAL;

        public UserRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig,
            IOptions<MailConfig> mailConfig, ILogger<UserRepository> logger)
        {
            _HttpContext = context;
            _logger = logger;
            _UserDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _userPositionDAL = new UserPositionDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _userRoleDAL = new UserRoleDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _userDepart = new UserDepartDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _departmentDAL = new DepartmentDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<UserDetailViewModel> CheckExistAccount(AccountModel entity)
        {
            try
            {
                var _encryptPassword = EncodeHelpers.MD5Hash(entity.Password);
                var _model = await _UserDAL.GetByUserName(entity.UserName);
                if (_model != null)
                {
                    if (_encryptPassword == _model.Password || _encryptPassword == _model.ResetPassword)
                    {
                        if (_model.Password != _model.ResetPassword)
                        {
                            if (_encryptPassword == _model.Password)
                            {
                                _model.ResetPassword = _encryptPassword;
                            }
                            else
                            {
                                _model.Password = _encryptPassword;
                            }

                            await _UserDAL.UpdateAsync(_model);
                        }

                        return await GetDetailUser(_model.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckExistAccount - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return null;
        }

        public async Task<bool> ResetPassword(string input)
        {
            try
            {
                User _model;
                if (StringHelpers.IsValidEmail(input))
                {
                    _model = await _UserDAL.GetByEmail(input);
                }
                else
                {
                    _model = await _UserDAL.GetByUserName(input);
                }

                if (_model != null)
                {
                    var _Password = StringHelpers.CreateRandomPassword();
                    _model.ResetPassword = EncodeHelpers.MD5Hash(_Password);
                    await _UserDAL.UpdateAsync(_model);

                    var _Subject = "Tin nhắn từ hệ thống";
                    var _Body = "Mật khẩu đăng nhập của bạn vừa được thay đổi: <b>" + _Password + "</b>";
                    // await EmailHelper.SendMailAsync(_model.Email, _Subject, _Body, string.Empty, string.Empty);

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPassword - UserRepository: " + ex);
                _logger.LogError("ResetPassword: " + ex.Message);
            }
            return false;
        }



        public async Task<List<RolePermission>> GetUserPermissionById(int Id)
        {
            try
            {
                return await _UserDAL.GetUserPermissionById(Id);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<UserDetailViewModel> GetDetailUser(int Id)
        {
            var model = new UserDetailViewModel();
            try
            {
                model.Entity = await _UserDAL.FindAsync(Id);
                model.RoleIdList = await _userRoleDAL.GetUserRoleId(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailUser - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return model;
        }

        public async Task<UserDataViewModel> GetUser(int Id)
        {
            var model = new UserDataViewModel();
            try
            {
                var user= await _UserDAL.GetById(Id);
                if (user == null || user.Id <= 0) return model;
                model = JsonConvert.DeserializeObject<UserDataViewModel>(JsonConvert.SerializeObject(user));
                if(model.DepartmentId!=null && model.DepartmentId > 0)
                {
                    var depart = await _departmentDAL.GetById((int)model.DepartmentId);
                    if (depart != null && depart.Id > 0) model.DepartmentName = depart.DepartmentName;
                }
                model.RoleIdList = await _userRoleDAL.GetUserRoleId(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailUser - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return model;
        }

        public List<UserGridModel> GetPagingList(string userName,  int? status, int currentPage, int pageSize)
        {
            var model = new List<UserGridModel>();
            
            try
            {
                
                model = _UserDAL.GetUserPagingList(userName,  status, currentPage, pageSize);
                
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - UserRepository: " + ex);
            }
            return model;
        }

        public async Task<int> Create(UserViewModel model)
        {
            try
            {
                var user_claim_id = _HttpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var user_id = 0;
                if (user_claim_id != null) int.TryParse(user_claim_id.Value, out user_id);

                var entity = new User()
                {
                    UserName = StringHelpers.ConvertStringToNoSymbol(model.UserName.ToLower()).Replace(" ", ""),
                    FullName = model.FullName,
                    Password = EncodeHelpers.MD5Hash(model.Password),
                    ResetPassword = EncodeHelpers.MD5Hash(model.Password),
                    Phone = model.Phone ?? "",
                    BirthDay = !string.IsNullOrEmpty(model.BirthDayPicker)
           ? DateTime.ParseExact(model.BirthDayPicker,
                                 new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" },
                                 CultureInfo.InvariantCulture,
                                 DateTimeStyles.None)
           : model.BirthDay,

                    Gender = model.Gender,
                    Email = model.Email,
                    Avata = model.Avata,
                    Address = model.Address ?? "",
                    Status = model.Status,
                    DepartmentId = model.DepartmentId,
                    Note = model.Note ?? "",
                    CreatedBy = user_id,
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = user_id,
                    UserPositionId = model.UserPositionId,
                    Level = model.Level,
                    //Id = model.Id,
                    Manager = model.Manager,
                    UserMapId = model.UserMapId,
                    NickName = model.NickName,
                    DebtLimit = model.DebtLimit
                };
                // ✅ BỔ SUNG DÒNG NÀY
                //entity.Id = 0;

                // Check exist User Name or Email
                var exmodel = await _UserDAL.GetByUserName(model.UserName);
               
                if (exmodel != null && exmodel.Id > 0)
                {
                    return -1;
                }

                var userId = await  _UserDAL.CreateAsync(entity);

                if (!string.IsNullOrEmpty(model.RoleId))
                {
                    var role_list = model.RoleId.Split(',').Select(s => int.Parse(s)).ToArray();
                    foreach (var role in role_list) {
                         _userRoleDAL.UpsertUserRole(new UserRole()
                        {
                            UserId = entity.Id,
                            RoleId = role
                        });
                    
                    }
                    var exists_roles = await _userRoleDAL.GetUserRoleId(entity.Id);
                    if (exists_roles.Any())
                    {
                        var non_keep = exists_roles.Where(x => !role_list.Contains(x)).ToArray();
                        _userRoleDAL.DeleteUserRole(entity.Id, non_keep);
                    }
                }
                return Convert.ToInt32(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                LogHelper.InsertLogTelegram("Create - UserRepository: " + ex);
            }

            return 0;
        }

        public async Task<int> DeleteUserRole(int userId, int[] arrayRole)
        {
            try
            {

                _userRoleDAL.DeleteUserRole(userId, arrayRole);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteUserRole - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return 0;
        }
        public async Task<int> UpdateUserRole(int userId, int[] arrayRole)
        {
            try
            {

                if (arrayRole != null && arrayRole.Count() > 0)
                {
                    foreach (var role in arrayRole)
                    {
                        _userRoleDAL.UpsertUserRole(new UserRole()
                        {
                            UserId = userId,
                            RoleId = role
                        });

                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateUserRole - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return 0;
        }
        public async Task<int> ChangeUserStatus(int userId)
        {
            try
            {
                var model = await _UserDAL.FindAsync(userId);
                if (model.Status == 0)
                {
                    model.Status = 1;
                }
                else
                {
                    model.Status = 0;
                }
                await _UserDAL.UpdateAsync(model);
                return (int)model.Status;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeUserStatus - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return -1;
        }

        public async Task<User> FindById(int id)
        {
            var model = new User();
            try
            {
                model = await _UserDAL.FindAsync(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindById - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return model;
        }

        public async Task<int> Update(UserViewModel model)
        {
            try
            {
                var entity = await _UserDAL.FindAsync(model.Id);

                // Check exist User Name or Email
                // var userList = await _UserDAL.GetAllAsync();

                //var exmodel = userList.Where(s => s.Id != entity.Id /*&& s.Status == 0 && (s.UserName == entity.UserName || s.Email == entity.Email)*/).FirstOrDefault();
                //if (exmodel != null && exmodel.Count > 0)
                //{
                //    return -1;
                //}
                //var exists_other_user = await _UserDAL.CheckIfExists(model.UserName, model.Id);
                //if(exists_other_user!=null && exists_other_user.Id > 0)
                //{
                //    return -1;
                //}
                if (entity.DepartmentId.HasValue && entity.DepartmentId.Value > 0
                    && model.DepartmentId.HasValue && model.DepartmentId.Value > 0
                    && entity.DepartmentId.Value != model.DepartmentId.Value)
                {
                    var user_claim_id = _HttpContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                    var user_id = 0;
                    if (user_claim_id != null) int.TryParse(user_claim_id.Value, out user_id);
                    await _UserDAL.LogDepartmentOfUser(entity, user_id);
                }

                entity.FullName = model.FullName;
                entity.Phone = model.Phone ?? "";
                entity.BirthDay = !string.IsNullOrEmpty(model.BirthDayPicker) ?
                                  DateTime.ParseExact(model.BirthDayPicker, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                                 : model.BirthDay;
                entity.Gender = model.Gender;
                entity.Email = model.Email;
                if (model.Avata != null)
                {
                    entity.Avata = model.Avata;
                }
                entity.Address = model.Address ?? "";
                entity.Status = model.Status;
                entity.Note = model.Note ?? "";
                entity.ModifiedBy = 1;
                entity.DepartmentId = model.DepartmentId;
                entity.ModifiedOn = DateTime.Now;
                entity.UserPositionId = model.UserPositionId;
                entity.Level = model.Level == null ? entity.Level : model.Level;
                entity.NickName = model.NickName;
                entity.DebtLimit = model.DebtLimit;


               await  _UserDAL.UpdateAsync(entity);
                if (!string.IsNullOrEmpty(model.RoleId))
                {
                    var role_list = model.RoleId.Split(',').Select(s => int.Parse(s)).ToArray();
                    foreach (var role in role_list)
                    {
                        _userRoleDAL.UpsertUserRole(new UserRole()
                        {
                            UserId = entity.Id,
                            RoleId = role
                        });

                    }
                    var exists_roles = await  _userRoleDAL.GetUserRoleId(entity.Id);   
                    if (exists_roles.Any()) {
                        var non_keep = exists_roles.Where(x => !role_list.Contains(x)).ToArray();
                        _userRoleDAL.DeleteUserRole(entity.Id, non_keep);
                    }
                }
                return model.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return 0;
        }

        public async Task<List<User>> GetUserSuggestionList(string name)
        {
            List<User> data = new List<User>();
            try
            {
                data = await _UserDAL.GetAllAsync();
                if (!string.IsNullOrEmpty(name))
                {
                    data = data.Where(s => s.UserName.Trim().ToLower().Contains(StringHelpers.ConvertStringToNoSymbol(name.Trim().ToLower()))).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggestionList - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return data;
        }

        public async Task<string> ResetPasswordByUserId(int userId)
        {
            var rs = string.Empty;
            try
            {
                var _model = await _UserDAL.FindAsync(userId);
                var _newPassword = StringHelpers.CreateRandomPassword();
                _model.ResetPassword = EncodeHelpers.MD5Hash(_newPassword);
                _model.Password = EncodeHelpers.MD5Hash(_newPassword);
                await _UserDAL.UpdateAsync(_model);
                rs = _newPassword;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ResetPasswordByUserId - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return rs;
        }

        public async Task<int> ChangePassword(UserPasswordModel model)
        {
            var rs = 0;
            try
            {
                var _model = await _UserDAL.FindAsync(model.Id);
                if (_model.Password == EncodeHelpers.MD5Hash(model.Password))
                {
                    _model.ResetPassword = EncodeHelpers.MD5Hash(model.NewPassword);
                    _model.Password = EncodeHelpers.MD5Hash(model.NewPassword);
                    await _UserDAL.UpdateAsync(_model);
                    rs = model.Id;
                }
                else
                {
                    rs = -1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangePassword - UserRepository: " + ex);
                _logger.LogError(ex.Message);
            }
            return rs;
        }
        public List<User> GetAll()
        {
            try
            {
                return _UserDAL.GetAll();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<User>> GetUserSuggesstion(string txt_search)
        {
            try
            {
                if (txt_search == null) txt_search = "";
                return await _UserDAL.GetUserSuggesstion(txt_search);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggesstion - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<List<User>> GetUserSuggesstion(string txt_search, List<int> ids)
        {
            try
            {
                if (txt_search == null) txt_search = "";
                return await _UserDAL.GetUserSuggesstion(txt_search, ids);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserSuggesstion - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<User> GetClientDetailAsync(long clientId)
        {
            try
            {

                return _UserDAL.GetUserIdById(clientId);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<User> GetById(long userIds)
        {
            try
            {

                return await _UserDAL.GetById(userIds);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetail - ClientDAL: " + ex);
                return null;
            }
        }
        public List<UserPosition> GetUserPositions()
        {
            try
            {
                return _userPositionDAL.GetAll();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserPositions - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<UserPosition> GetUserPositionsByID(int id)
        {
            try
            {
                return await _userPositionDAL.GetById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserPositionsByID - ClientDAL: " + ex);
                return null;
            }
        }
        public async Task<List<Role>> GetUserActiveRoleList(int user_id)
        {
            try
            {
                return await _userRoleDAL.GetUserActiveRoleList(user_id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserActiveRoleList - ClientDAL: " + ex);
                return null;
            }
        }

        public string GetListUserByUserId(int user_id)
        {
            try
            {
                return _UserDAL.GetListUserByUserId(user_id);
            }
            catch
            {
                throw;
            }
        }

        //các chức năng của 1 quyền mà user đó dc phép  dùng
       
        public async Task<int> GetManagerByUserId(long UserId)
        {
            try
            {
                var data = await _userRoleDAL.GetManagerByUserId(UserId);

                return data;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetManagerByUserId - UserRepository: " + ex);
                return 0;
            }
        }
     

    }
}
