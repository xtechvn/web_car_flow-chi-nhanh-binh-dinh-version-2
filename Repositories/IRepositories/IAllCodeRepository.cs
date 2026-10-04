using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllCodeRepository
    {
        List<AllCode> GetAll();
        List<AllCode> GetListAllAsync();
        Task<AllCode> GetById(int Id);
        Task<long> Create(AllCode model);
        Task<long> Update(AllCode model);
        Task<long> Delete(int id);
        List<AllCode> GetListByType(string type);

        AllCode GetByType(string type);
        Task<short> GetLastestCodeValueByType(string type);
        Task<short> GetLastestOrderNoByType(string type);
        Task<AllCode> GetIDIfValueExists(string type, string description);
        Task<List<AllCode>> GetListSortByName(string type_name);
        Task<T> GetAllCodeValueByType<T>(string apiPrefix, string keyToken, string key, string type);
        Task<T> Sendata<T>(string apiPrefix, string keyToken, Dictionary<string,string> keyValuePairs);

        Task<long> UpdateAllCode(AllCode model);
        Task<long> InsertAllcode(AllCode model);
        Task<long> UpdateResetTime(int Id, DateTime? time);
    }
}
