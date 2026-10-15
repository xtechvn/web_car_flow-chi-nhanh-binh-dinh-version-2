using DAL.StoreProcedure;
using Entities.ViewModels.ObdConfig;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ObdConfigDAL
    {
        private static DbWorker _DbWorker;
        public ObdConfigDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<DateTime?> GetBusinessDate()
        {
            try
            {
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetObdBusinessDate);
                if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["BusinessDate"] != DBNull.Value)
                {
                    return Convert.ToDateTime(dt.Rows[0]["BusinessDate"]);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetBusinessDate - ObdConfigDAL: " + ex);
            }
            return null;
        }

        public async Task<ObdConfigSaveResult> InsertFromExcel(List<ObdConfigImportRow> rows, int userId, string fileName)
        {
            try
            {
                var table = new DataTable();
                table.Columns.Add("RowNumber", typeof(int));
                table.Columns.Add("Delivery", typeof(long));
                table.Columns.Add("NetWeight", typeof(decimal));
                foreach (var row in rows)
                {
                    table.Rows.Add(row.RowNumber, row.Delivery, row.NetWeight);
                }

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Rows", SqlDbType.Structured) { TypeName = "dbo.ObdConfigImportType", Value = table },
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@FileName", (object)fileName ?? DBNull.Value),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_InsertObdConfigFromExcel, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigSaveResult>().First();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertFromExcel - ObdConfigDAL: " + ex);
            }
            return null;
        }

        public async Task<ObdConfigSaveResult> Insert(long delivery, decimal netWeight, int userId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Delivery", delivery),
                    new SqlParameter("@NetWeight", SqlDbType.Decimal) { Precision = 18, Scale = 3, Value = netWeight },
                    new SqlParameter("@UserId", userId),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_InsertObdConfig, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigSaveResult>().First();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - ObdConfigDAL: " + ex);
            }
            return null;
        }

        public async Task<ObdConfigSaveResult> UpdateDelivery(long obdId, long delivery, int userId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@ObdId", obdId),
                    new SqlParameter("@Delivery", delivery),
                    new SqlParameter("@UserId", userId),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_UpdateObdConfig, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigSaveResult>().First();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateDelivery - ObdConfigDAL: " + ex);
            }
            return null;
        }

        public async Task<List<ObdConfigVersionModel>> GetListVersion(DateTime loadingDate)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@LoadingDate", SqlDbType.Date) { Value = loadingDate.Date },
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListObdConfigVersion, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigVersionModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVersion - ObdConfigDAL: " + ex);
            }
            return new List<ObdConfigVersionModel>();
        }

        public async Task<List<ObdConfigModel>> GetList(DateTime loadingDate, int version, string delivery, int pageIndex, int pageSize)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@LoadingDate", SqlDbType.Date) { Value = loadingDate.Date },
                    new SqlParameter("@Version", version),
                    new SqlParameter("@Delivery", string.IsNullOrWhiteSpace(delivery) ? DBNull.Value : delivery.Trim()),
                    new SqlParameter("@PageIndex", pageIndex),
                    new SqlParameter("@PageSize", pageSize),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListObdConfig, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - ObdConfigDAL: " + ex);
            }
            return new List<ObdConfigModel>();
        }

        public ObdConfigModel GetById(long obdId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@ObdId", obdId),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetObdConfigById, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigModel>().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ObdConfigDAL: " + ex);
            }
            return null;
        }

        public List<ObdConfigLogModel> GetListLog(long obdId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@ObdId", obdId),
                };
                var dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListObdConfigLog, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.ToList<ObdConfigLogModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListLog - ObdConfigDAL: " + ex);
            }
            return new List<ObdConfigLogModel>();
        }
    }
}
