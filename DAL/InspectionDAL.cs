using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Contants;
using Utilities;
using Microsoft.Data.SqlClient;
using Entities.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class InspectionDAL : GenericService<Inspection>
    {
        private static DbWorker _DbWorker;
        public InspectionDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<List<InspectionModel>> GetListInspection(InspectionSearchModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@VehicleNumber", searchModel.VehicleNumber==null? DBNull.Value :searchModel.VehicleNumber),
                    new SqlParameter("@Type", searchModel.Type),

                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListInspection, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<InspectionModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListCartoFactory - VehicleInspectionDAL: " + ex);
            }
            return null;
        }
        public async Task<InspectionModel> GetDetailInspectionById(int id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                   
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailInspectionById, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data= dt.ToList<InspectionModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListCartoFactory - VehicleInspectionDAL: " + ex);
            }
            return null;
        }
        public async Task<int> InsertInspection(Inspection model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                 {
                        new SqlParameter("@VehicleNumber", model.VehicleNumber),
                        new SqlParameter("@ExpirationDate", model.ExpirationDate),
                        new SqlParameter("@VehicleWeight", model.VehicleWeight),
                        new SqlParameter("@InspectionDate", model.InspectionDate),
                        new SqlParameter("@VehicleWeightMax", model.VehicleWeightMax),

                 };
                var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertInspection, objParam);
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertInspection - VehicleInspectionDAL: " + ex);
            }
            return -1;
        }
        public async Task<int> UpdateInspection(Inspection model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {

                    new SqlParameter("@Id", model.Id),
                    new SqlParameter("@VehicleNumber", model.VehicleNumber),
                    new SqlParameter("@ExpirationDate", model.ExpirationDate),
                    new SqlParameter("@VehicleWeight", model.VehicleWeight),
                    new SqlParameter("@InspectionDate", model.InspectionDate),
                    new SqlParameter("@VehicleWeightMax", model.VehicleWeightMax),
                };
                var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateInspection, objParam);
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateInspection - VehicleInspectionDAL: " + ex);
            }
            return -1;
        }   
        public async Task<Inspection> CheckVehicleNumber(string VehicleNumber)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Inspections.AsNoTracking().FirstOrDefaultAsync(s => s.VehicleNumber.Equals(VehicleNumber));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckVehicleNumber - VehicleInspectionDAL: " + ex);
            }
            return null;
        }
    }
}
