using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class UserRoleDAL : GenericService<UserRole>
    {
        private static DbWorker _DbWorker;

        public UserRoleDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
       
        public async Task<List<Role>> GetUserActiveRoleList(int user_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var list_role_id = await _DbContext.UserRoles.Where(s => s.UserId == user_id).ToListAsync();
                    if (list_role_id != null && list_role_id.Count > 0)
                    {
                        return await _DbContext.Roles.Where(s => list_role_id.Select(x => x.RoleId).Contains(s.Id)).ToListAsync();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - UserPositionDAL: " + ex);
                return null;
            }
        }
        public async Task<List<RolePermissionViewModel>> GetListRolePermissionByUserAndRole(long UserId, List<long> RoleIds)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@UserId", UserId);
                objParam[1] = new SqlParameter("@RoleId", string.Join(',', RoleIds));
                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListRolePermissionByUserAndRole, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    return data;
                }

                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return null;
            }
        }
        public async Task<int> GetManagerByUserId(long UserId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@UserId", UserId);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetManagerByUserId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    var id = Convert.ToInt32(dt.Rows[0]["UserId"]);
                    return id;
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return 0;
            }
        }
        public async Task<int> GetLeaderByUserId(long UserId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@UserId", UserId);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.Sp_GetLeaderByUserId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<RolePermissionViewModel>();
                    var id = Convert.ToInt32(dt.Rows[0]["UserId"]);
                    return id;
                }

                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRolePermissionByUserAndRole - UserRoleDAL. " + ex);
                return 0;
            }
        }
      


        public async Task<List<int>> GetUserRoleId(int userId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.UserRoles.Where(x => x.UserId == userId).Select(s => s.RoleId).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetUserRoleId - UserRoleDAL: " + ex);
                return new List<int>();
            }
        }
       
        public List<User> GetListUserByRole(int role_id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var user_role = _DbContext.UserRoles.Where(s => s.RoleId == role_id).ToList();
                    var userRoleIds = user_role.Select(n => n.UserId).ToList();
                    if (userRoleIds.Count > 0)
                    {
                        return _DbContext.Users.Where(s => userRoleIds.Contains(s.Id) && s.Status == 0).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListUserByRole - UserRoleDAL: " + ex);
            }
            return new List<User>();
        }
        public int UpsertUserRole(UserRole role)
        {
            try
            {

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", role.UserId),
                    new SqlParameter("@UserRole", role.RoleId)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.UpsertUserRole, parameters);
                role.Id = id;
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUserRole - UserDAL: " + ex);
                return -1;
            }
        }
        public int DeleteUserRole(int user_id, int[] roles)
        {
            try
            {
                string role_string = "";
                if(roles != null && roles.Count() > 0)
                {
                    role_string = string.Join(",", roles);
                }
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@UserID", user_id),
                    new SqlParameter("@UserRole",role_string)
                };
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.DeleteUserRole, parameters);
                
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpsertUserRole - UserDAL: " + ex);
                return -1;
            }
        }
    }
}