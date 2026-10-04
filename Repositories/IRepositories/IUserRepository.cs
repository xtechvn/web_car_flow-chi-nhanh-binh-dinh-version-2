using Entities.Models;
using Entities.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<UserDetailViewModel> CheckExistAccount(AccountModel entity);
        Task<bool> ResetPassword(string input);
        List<UserGridModel> GetPagingList(string userName,  int? status, int currentPage, int pageSize);
        Task<UserDetailViewModel> GetDetailUser(int Id);
        Task<UserDataViewModel> GetUser(int Id);
        Task<int> Create(UserViewModel model);
        Task<int> Update(UserViewModel model);
        Task<int> UpdateUserRole(int userId, int[] arrayRole);
        Task<int> DeleteUserRole(int userId, int[] arrayRole);
        Task<int> ChangeUserStatus(int userId);
        Task<User> FindById(int id);
        Task<List<User>> GetUserSuggestionList(string userName);
        Task<User> GetById(long userIds);
        Task<string> ResetPasswordByUserId(int userId);
        Task<int> ChangePassword(UserPasswordModel model);
        List<User> GetAll();
        Task<List<User>> GetUserSuggesstion(string txt_search);
        Task<List<User>> GetUserSuggesstion(string txt_search, List<int> ids);
        Task<User> GetClientDetailAsync(long clientId);

        List<UserPosition> GetUserPositions();
        Task<UserPosition> GetUserPositionsByID(int id);
        Task<List<Role>> GetUserActiveRoleList(int user_id);
        string GetListUserByUserId(int user_id);
        Task<int> GetManagerByUserId(long UserId);
        Task<List<RolePermission>> GetUserPermissionById(int Id);

    }
}
