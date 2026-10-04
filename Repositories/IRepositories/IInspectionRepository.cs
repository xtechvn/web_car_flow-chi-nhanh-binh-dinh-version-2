using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IInspectionRepository
    {
        Task<List<InspectionModel>> GetListInspection(InspectionSearchModel searchModel);
        Task<InspectionModel> GetDetailInspectionById(int id);
        Task<int> InsertInspection(Entities.Models.Inspection model);
        Task<int> UpdateInspection(Entities.Models.Inspection model);
        Task<Inspection> CheckVehicleNumber(string VehicleNumber);
    }
}
