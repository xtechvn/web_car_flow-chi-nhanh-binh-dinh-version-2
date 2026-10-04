using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.IRepositories
{
    public interface IMongoService
    {
        Task<long> Insert(RegistrationRecord model);
        Task<int> CheckPlateNumber(string PlateNumber);
        List<RegistrationRecordMongo> GetList();
    }
}
