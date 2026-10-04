using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class UserDAL : GenericService<User>
    {
        private static DbWorker _DbWorker;
        public UserDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public List<UserGridModel> GetUserPagingList(string name, int? status, int page_index, int page_size)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Name", ((name==null||name.Trim()=="")? DBNull.Value: name)),
                    new SqlParameter("@Status", (status==null? DBNull.Value: (int)status)),
                    new SqlParameter("@PageIndex", page_index),
                    new SqlParameter("@PageSize",page_size)
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllUser_search, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<UserGridModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserPagingList - UserDAL: " + ex);
            }
            return null;
        }

    

        public async Task<User> GetByUserName(string input)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => s.UserName.Equals(input));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }

        public async Task<List<User>> GetByIds(List<long> userIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().Where(s => userIds.Contains(s.Id)).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByIds - UserDAL: " + ex);
                return new List<User>();
            }
        }

        public async Task<User> GetById(long userIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => userIds == s.Id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByIds - UserDAL: " + ex);
                return null;
            }
        }

       

        public async Task<User> GetByEmail(string input)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => s.Email.Equals(input));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }

     
        public User GetUserIdById(long Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Users.AsNoTracking().FirstOrDefault(s => s.Id == Id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserIdById - UserDAL: " + ex);
                return null;
            }

        }
        public List<User> GetAll()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Users.AsNoTracking().ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAll - UserDAL: " + ex);
                return null;
            }
        }
        public async Task<List<User>> GetUserSuggesstion(string txt_search)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().Where(s => s.UserName.ToLower().Contains(txt_search.ToLower())).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }

        public async Task<List<RolePermission>> GetUserPermissionById(int user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                     SqlParameter[] objParam = new SqlParameter[]
                     {
                        new SqlParameter("@UserId", user_id)
                     };
                    var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListUserPermissionByUserId, objParam);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        return dt.ToList<RolePermission>();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetMenuPermissionByUserId - UserDAL: " + ex);
            }
            return null;

        }

        public async Task LogDepartmentOfUser(User model, int current_user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_department_model = await _DbContext.UserDeparts.Where(s => s.UserId == model.Id)
                        .OrderByDescending(s => s.Id)
                        .FirstOrDefaultAsync();

                    var dataJoinTime = model.CreatedOn;

                    if (user_department_model != null)
                    {
                        dataJoinTime = user_department_model.LeaveDate;
                    }

                    _DbContext.UserDeparts.Add(new UserDepart
                    {
                        UserId = model.Id,
                        DepartmentId = model.DepartmentId,
                        JoinDate = dataJoinTime,
                        LeaveDate = DateTime.Now,
                        CreatedBy = current_user_id,
                        CreatedDate = DateTime.Now
                    });

                    await _DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LogDepartmentOfUser - UserDAL: " + ex);
                throw;
            }
        }

        public string GetListUserByUserId(int user_id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@UserId", user_id);
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListUserByUserId, objParam);

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows[0]["ListUserId"].ToString();
                }
                else
                {
                    return user_id.ToString();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - PolicyDal: " + ex);
                throw;
            }
        }
        public async Task<User> GetChiefofDepartmentByRoleID(int role_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_role = await _DbContext.UserRoles.Where(s => s.RoleId == role_id).FirstOrDefaultAsync();
                    if (user_role != null && user_role.Id > 0)
                    {
                        return await _DbContext.Users.AsNoTracking().Where(s => s.Id == user_role.UserId && s.Status == 0).FirstOrDefaultAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetChiefofDepartmentByServiceType - PolicyDal: " + ex);
            }
            return null;
        }

        public async Task<List<User>> GetListChiefofDepartmentByRoleID(int role_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_role = _DbContext.UserRoles.Where(s => s.RoleId == role_id).ToList();
                    var listUserId = user_role.Select(n => n.UserId).ToList();
                    if (listUserId.Count > 0)
                    {
                        return await _DbContext.Users.AsNoTracking().Where(s => listUserId.Contains(s.Id) && s.Status == 0).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetChiefofDepartmentByServiceType - PolicyDal: " + ex);
            }
            return new List<User>();
        }

        public async Task<List<User>> GetListChiefofDepartmentByRoleID(List<int> role_ids)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_role = _DbContext.UserRoles.Where(s => role_ids.Contains(s.RoleId)).ToList();
                    var listUserId = user_role.Select(n => n.UserId).ToList();
                    if (listUserId.Count > 0)
                    {
                        return await _DbContext.Users.AsNoTracking().Where(s => listUserId.Contains(s.Id) && s.Status == 0).ToListAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetChiefofDepartmentByServiceType - PolicyDal: " + ex);
            }
            return new List<User>();
        }
       
        public async Task<List<User>> GetUserSuggesstion(string txt_search,List<int> ids)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().Where(s => s.UserName.ToLower().Contains(txt_search.ToLower()) && ids.Contains(s.Id)).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }
        public async Task<List<User>> GetByDepartmentIds(List<int?> ids)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().Where(s => ids.Contains(s.DepartmentId)).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByDepartmentIds - UserDepartDAL: " + ex);
                return null;
            }
        }
        public int UpsertUser(User user)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", user.Id),
                    new SqlParameter("@UserName", user.UserName),
                    new SqlParameter("@FullName", user.FullName),
                    new SqlParameter("@Password", user.Password) ,
                    new SqlParameter("@ResetPassword",user.ResetPassword),
                    new SqlParameter("@Phone", user.Phone),
                    new SqlParameter("@BirthDay", user.BirthDay),
                    new SqlParameter("@Gender", user.Gender),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@Avata", user.Avata),
                    new SqlParameter("@Address",user.Address),
                    new SqlParameter("@Status", user.Status),
                    new SqlParameter("@Note",user.Note),
                    new SqlParameter("@CreatedBy",user.CreatedBy),
                    new SqlParameter("@CreatedOn", user.CreatedOn),
                    new SqlParameter("@ModifiedBy", user.ModifiedBy),
                    new SqlParameter("@ModifiedOn", user.ModifiedOn)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpsertUser, parameters);
                user.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUser - UserDAL: " + ex);
                return -1;
            }
        }
        public List<int> GetListUserIdByRole(string strRoleId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var arrRoleId = strRoleId.Split(',').Select(s => int.Parse(s)).ToArray();
                    return _DbContext.UserRoles.Where(s => arrRoleId.Contains(s.RoleId)).Select(s => s.UserId).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserIdByRole - UserRoleDAL: " + ex);
                return new List<int>();
            }

        }
        public async Task<User> CheckIfExists(string user_name,int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Users.AsNoTracking().FirstOrDefaultAsync(s => s.UserName.Equals(user_name) && s.Id!=id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByUserName - UserDAL: " + ex);
                return null;
            }
        }
 
    }
}