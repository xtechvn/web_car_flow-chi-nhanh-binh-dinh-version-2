using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Car;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly InspectionDAL _inspectionDAL;
      
        public InspectionRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _inspectionDAL = new InspectionDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            
        }
        public async Task<List<InspectionModel>> GetListInspection(InspectionSearchModel searchModel)
        {
            try
            {
                return await _inspectionDAL.GetListInspection(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInspection - InspectionRepository: " + ex);
            }
            return null;
        }  
        public async Task<InspectionModel> GetDetailInspectionById(int id)
        {
            try
            {
                return await _inspectionDAL.GetDetailInspectionById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInspection - InspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<int> InsertInspection(Inspection model)
        {
            try
            {

                return await _inspectionDAL.InsertInspection(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertInspection - InspectionRepository: " + ex);
            }
            return -1;
        }
        public async Task<int> UpdateInspection(Inspection model)
        {
            try
            {

                return await _inspectionDAL.UpdateInspection(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateInspection - InspectionRepository: " + ex);
            }
            return -1;
        }
        public async Task<Inspection> CheckVehicleNumber(string VehicleNumber)
        {
            try
            {

                return await _inspectionDAL.CheckVehicleNumber(VehicleNumber);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateInspection - InspectionRepository: " + ex);
            }
            return null;
        }
    }
}
