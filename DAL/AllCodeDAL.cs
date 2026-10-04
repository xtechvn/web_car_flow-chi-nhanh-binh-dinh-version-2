using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class AllCodeDAL : GenericService<AllCode>
    {
        private DbWorker dbWorker;
        public AllCodeDAL(string connection) : base(connection) {

            dbWorker = new DbWorker(connection);
        }

        public List<AllCode> GetListByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Set<AllCode>().Where(n => n.Type == type).ToList();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListByType - AllCodeDAL. " + ex);
                return null;
            }
        }

        public AllCode GetByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Set<AllCode>().Where(n => n.Type == type).FirstOrDefault();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByType - AllCodeDAL. " + ex);
                return null;
            }
        }
        public async Task<AllCode> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.AllCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return await detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - AllCodeDAL: " + ex);
                return null;
            }
        }
        public async Task<short> GetLastestCodeValueByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCodes.AsNoTracking().Where(x => x.Type == type).OrderByDescending(x => x.CodeValue).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return  detail.CodeValue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestCodeValueByType - AllCodeDAL: " + ex);
            }
            return -1;
        }
        public async Task<short> GetLastestOrderNoByType(string type)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCodes.AsNoTracking().Where(x => x.Type == type).OrderByDescending(x => x.OrderNo).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return detail.OrderNo!=null?(short)detail.OrderNo: (short)-1;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLastestOrderNoByType - AllCodeDAL: " + ex);
            }
            return -1;
        }
        public async Task<AllCode> GetIDIfValueExists(string type, string description)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCodes.AsNoTracking().Where(x => x.Type == type && x.Description==description).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetIDIfValueExists - AllCodeDAL: " + ex);
            }
            return null;
        }
        public async Task<List<AllCode>> GetListSortByName(string type_name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCodes.AsNoTracking().Where(n => n.Type == type_name).OrderBy(x=>x.Description).ToListAsync();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListSortByName - AllCodeDAL. " + ex);
                return null;
            }
        }
        public async Task<AllCode> GetIfDescriptionExists(string type, string description)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AllCodes.AsNoTracking().Where(x => x.Type == type && x.Description.ToLower().Contains(description.Trim().ToLower())).FirstOrDefaultAsync();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetIDIfValueExists - AllCodeDAL: " + ex);
            }
            return null;
        }

        public async Task<long> InsertAllcode(AllCode model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[7];
                objParam[0] = new SqlParameter("@Type", model.Type);
                objParam[1] = new SqlParameter("@CodeValue", model.CodeValue);
                objParam[2] = new SqlParameter("@Description", model.Description);
                objParam[3] = new SqlParameter("@OrderNo", model.OrderNo);
                objParam[4] = new SqlParameter("@CreatedBy", model.CreatedBy);
                objParam[5] = new SqlParameter("@CreateDate", DBNull.Value);
                objParam[6] = new SqlParameter("@UpdateTime", DBNull.Value);
                return dbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertAllcode, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertAllcode - AllCodeDAL. " + ex);
                return -1;
            }
        }
        public async Task<long> UpdateAllCode(AllCode model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[8];
                objParam[0] = new SqlParameter("@Id", model.Id);
                objParam[1] = new SqlParameter("@Type", model.Type);
                objParam[2] = new SqlParameter("@CodeValue", model.CodeValue);
                objParam[3] = new SqlParameter("@Description", model.Description);
                objParam[4] = new SqlParameter("@OrderNo", model.OrderNo);
                objParam[5] = new SqlParameter("@CreateDate", DBNull.Value);
                objParam[6] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                objParam[7] = new SqlParameter("@UpdateTime", DBNull.Value);
                return dbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateAllCode, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAllCode - AllCodeDAL. " + ex);
                return -1;
            }
        } 
        
    }
}
