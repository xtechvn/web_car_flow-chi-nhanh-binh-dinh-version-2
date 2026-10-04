using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IVehicleInspectionRepository
    {
        Task<List<CartoFactoryModel>> GetListCartoFactory(CartoFactorySearchModel searchModel);
        Task<CartoFactoryModel> GetDetailtVehicleInspection(int id);
        Task<int> UpdateCar(VehicleInspectionUpdateModel model);
        int SaveVehicleInspection(RegistrationRecord model);
        Task<string> GetAudioPathByVehicleNumber(string VehicleNumber);
        Task<List<CartoFactoryModel>> GetListVehicleInspectionSynthetic(DateTime? FromDate, DateTime? ToDate, int LoadType);
        Task<TotalVehicleInspection> CountTotalVehicleInspectionSynthetic(DateTime? FromDate, DateTime? ToDate);
        Task<List<TotalWeightByHourModel>> GetTotalWeightByHour(DateTime? RegistrationTime);
        Task<List<TotalWeightByHourModel>> GetTotalWeightByWeightGroup(DateTime? RegistrationTime);
        Task<List<TotalWeightByHourModel>> GetTotalWeightByTroughType(DateTime? RegistrationTime);
        Task<List<CartoFactoryModel>> GetListRegisteredVehicle(CartoFactorySearchModel searchModel);

        Task<List<CartoFactoryModel>> GetListVehicleProcessingIsLoading(CartoFactorySearchModel searchModel);
        Task<List<CartoFactoryModel>> GetListVehicleWeighedInput(CartoFactorySearchModel searchModel);
        Task<List<CartoFactoryModel>> GetListVehicleCarCallList(CartoFactorySearchModel searchModel);
        Task<List<CartoFactoryModel>> GetListVehicleCallTheScale(CartoFactorySearchModel searchModel);
        Task<List<CartoFactoryModel>> GetListVehicleListVehicles(CartoFactorySearchModel searchModel);
        Task<List<TroughWeight>> GetListTroughWeightByVehicleInspectionId(int id);
        Task<int> InsertTroughWeight(TroughWeight model);
        Task<int> UpdateTroughWeight(TroughWeight model);
        Task<TroughWeight> GetDetailTroughWeightById(int id);
        Task<int> UpdateVehicleInspectionByVehicleNumber(string VehicleNumber);
        Task<int> UpdateVehicleLoadTaken(int Id, int VehicleLoadTaken);
        Task<string> ExportDeposit(CartoFactorySearchModel searchModel, string FilePath);
        Task<string> ExportSummaryReport(List<CartoFactoryModel> data, string FilePath);
        Task<string> ExportTimeProductDelivery(List<VehicleInspectionTimeProductDeliveryModel> data, string FilePath);
        Task<List<VehicleInspectionTimeProductDeliveryModel>> GetListVehicleInspectionTimeProductDelivery(DateTime? FromDate, DateTime? ToDate, int LoadType);

    }
}
