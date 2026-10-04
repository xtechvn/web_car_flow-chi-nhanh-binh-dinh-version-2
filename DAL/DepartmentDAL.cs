using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Report;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class DepartmentDAL : GenericService<Department>
    {
        private static DbWorker DbWorker;
        public DepartmentDAL(string connection) : base(connection)
        {
            DbWorker = new DbWorker(connection);
        }
        public async Task<DataTable> GetListRevenueByDepartment(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[17];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SalerId == null ? new SqlParameter("@SalerId", DBNull.Value) : new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[3] = searchModel.DepartmentId == 0 ? new SqlParameter("@DepartmentId", DBNull.Value) : new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[5] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[8] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[9] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[10] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[11] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[12] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[13] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[14] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                objParam[15] = new SqlParameter("@PermisionType", searchModel.PermisionType);
                objParam[16] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueByDepartment, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueByDepartment - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListRevenueBySaler(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[17];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SalerId == null ? new SqlParameter("@SalerId", DBNull.Value) : new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[3] = searchModel.DepartmentId == 0 ? new SqlParameter("@DepartmentId", DBNull.Value) : new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[5] =  new SqlParameter("@PageIndex", "-1");
                objParam[6] =  new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[8] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[9] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[10] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[11] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[12] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[13] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[14] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                objParam[15] = new SqlParameter("@PermisionType", searchModel.PermisionType);
                objParam[16] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueBySaler, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueBySaler - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListRevenueBySupplier(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[16];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SupplierId == 0 ? new SqlParameter("@SupplierId", DBNull.Value) : new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[3] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[4] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[5] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[6] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[7] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[8] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[9] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[10] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[11] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[12] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[13] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                objParam[14] = new SqlParameter("@PermisionType", searchModel.PermisionType);
                objParam[15] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueBySupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueBySupplier - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListRevenueByClient(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[16];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.ClientId == 0 ? new SqlParameter("@ClientId", DBNull.Value) : new SqlParameter("@ClientId", searchModel.ClientId);
                objParam[3] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[4] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[5] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[6] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[7] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[8] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[9] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[10] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[11] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[12] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[13] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                objParam[14] = new SqlParameter("@PermisionType", searchModel.PermisionType);
                objParam[15] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueByClient, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueByClient - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListDetailRevenueByDepartment(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[15];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SalerId == null ? new SqlParameter("@SalerId", DBNull.Value) : new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[3] = searchModel.DepartmentId == 0 ? new SqlParameter("@DepartmentId", DBNull.Value) : new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[5] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);

                objParam[8] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[9] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[10] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[11] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[12] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[13] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[14] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_DetailRevenueByDepartment, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueByDepartment - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListDetailRevenueBySaler(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[15];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SalerId == null ? new SqlParameter("@SalerId", DBNull.Value) : new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[3] = searchModel.DepartmentId == 0 ? new SqlParameter("@DepartmentId", DBNull.Value) : new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[5] =  new SqlParameter("@PageIndex", "-1");
                objParam[6] =  new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);

                objParam[8] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[9] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[10] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[11] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[12] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[13] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[14] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_DetailRevenueBySaler, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueBySaler - DepartmentDAL: " + ex);
            }
            return null;
        }

        public async Task<DataTable> GetListDetailRevenueBySupplier(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[14];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SupplierId == 0 ? new SqlParameter("@SupplierId", DBNull.Value) : new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[3] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[4] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[5] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[6] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);

                objParam[7] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[8] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[9] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[10] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[11] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[12] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[13] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_DetailRevenueBySupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueBySupplier - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListDetailRevenueByClient(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[14];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.ClientId == 0 ? new SqlParameter("@ClientId", DBNull.Value) : new SqlParameter("@ClientId", searchModel.ClientId);
                objParam[3] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[4] = searchModel.PageIndex == 0 ? new SqlParameter("@PageIndex", DBNull.Value) : new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[5] = searchModel.PageSize == -1 ? new SqlParameter("@PageSize", "20") : new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[6] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);

                objParam[7] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[8] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[9] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[10] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[11] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[12] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[13] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_DetailRevenueByClient, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueByClient - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetListOrder(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[18];
                objParam[0] = (CheckDate(searchModel.FromDate) == DateTime.MinValue) ? new SqlParameter("@FromDate", DBNull.Value) : new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = (CheckDate(searchModel.ToDate) == DateTime.MinValue) ? new SqlParameter("@ToDate", DBNull.Value) : new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = searchModel.SalerId == null ? new SqlParameter("@SalerId", DBNull.Value) : new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[3] = searchModel.DepartmentId == 0 ? new SqlParameter("@DepartmentId", DBNull.Value) : new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = searchModel.Vat == null ? new SqlParameter("@Vat", DBNull.Value) : new SqlParameter("@Vat", searchModel.Vat);
                objParam[5] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = searchModel.SalerPermission == null ? new SqlParameter("@SalerPermission", DBNull.Value) : new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                objParam[8] = searchModel.StartDateFromStr == null ? new SqlParameter("@StartDateFrom", DBNull.Value) : new SqlParameter("@StartDateFrom", searchModel.StartDateFrom);
                objParam[9] = searchModel.StartDateToStr == null ? new SqlParameter("@StartDateTo", DBNull.Value) : new SqlParameter("@StartDateTo", searchModel.StartDateTo);
                objParam[10] = searchModel.EndDateFromStr == null ? new SqlParameter("@EndDateFrom", DBNull.Value) : new SqlParameter("@EndDateFrom", searchModel.EndDateFrom);
                objParam[11] = searchModel.EndDateToStr == null ? new SqlParameter("@EndDateTo", DBNull.Value) : new SqlParameter("@EndDateTo", searchModel.EndDateTo);
                objParam[12] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[13] = (CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? new SqlParameter("@CreateDateFrom", DBNull.Value) : new SqlParameter("@CreateDateFrom", CheckDate(searchModel.CreateDateFrom));
                objParam[14] = (CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? new SqlParameter("@CreateDateTo", DBNull.Value) : new SqlParameter("@CreateDateTo", CheckDate(searchModel.CreateDateTo));
                objParam[15] = new SqlParameter("@PermisionType", searchModel.PermisionType);
                objParam[16] = new SqlParameter("@PaymentStatus", searchModel.PaymentStatus);
                objParam[17] = new SqlParameter("@Type", searchModel.Type);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_GetListOrder, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRevenueBySaler - DepartmentDAL: " + ex);
            }
            return null;
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
        public async Task<List<int>> GetAllDepartmentByParentDepartmentId(int department_id)
        {
            List<int> deparments = new List<int>();
            deparments.Add(department_id);
            try
            {
                
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var department_child = await _DbContext.Departments.AsNoTracking().Where(s => s.FullParent.Trim().Contains(department_id.ToString())).ToListAsync() ;
                    if(department_child!=null && department_child.Count > 0)
                    {
                        foreach(var de in department_child)
                        {
                            if(de.FullParent!=null && de.FullParent.Trim() != "")
                            {
                                var list = de.FullParent.Split(",").ToList();
                                if (list.Contains(department_id.ToString().Trim()))
                                {
                                    deparments.Add(de.Id);

                                }
                            }
                        }
                        deparments = deparments.Distinct().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllDepartmentByParentDepartmentId - DepartmentDAL: " + ex);
            }
            return deparments;
        }

        public async Task<DataTable> GetTableTotalDebtRevenueBySupplier(RevenueBySupplierViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[6];
                objParam[0] = new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = new SqlParameter("@Branch ", searchModel.Branch);
                objParam[3] = searchModel.SupplierId==0? new SqlParameter("@SupplierId",DBNull.Value):new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[4] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[5] = new SqlParameter("@PageSize", searchModel.PageSize);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalDebtRevenueBySupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTableTotalDebtRevenueBySupplier - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetTableDetailRevenueBySupplier(RevenueBySupplierViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[5];
                objParam[0] = new SqlParameter("@FromDate", CheckDate(searchModel.FromDate));
                objParam[1] = new SqlParameter("@ToDate", CheckDate(searchModel.ToDate));
                objParam[2] = new SqlParameter("@SupplierId", searchModel.SupplierId);
                objParam[3] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[4] = new SqlParameter("@PageSize", searchModel.PageSize);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_DetailDebtRevenueBySupplier, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTableTotalDebtRevenueBySupplier - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> TotalRevenueOrderBySale(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[8];
                objParam[0] = new SqlParameter("@FromDate", searchModel.StartDateFrom);
                objParam[1] = new SqlParameter("@ToDate", searchModel.StartDateTo);
                objParam[2] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[3] = new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[5] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueOrderBySale, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTableTotalDebtRevenueBySupplier - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<DataTable> TotalRevenueOrderByClient(ReportDepartmentViewModel searchModel)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[8];
                objParam[0] = new SqlParameter("@FromDate", searchModel.StartDateFrom);
                objParam[1] = new SqlParameter("@ToDate", searchModel.StartDateTo);
                objParam[2] = new SqlParameter("@OrderStatus", searchModel.OrderStatus);
                objParam[3] = new SqlParameter("@DepartmentId", searchModel.DepartmentId);
                objParam[4] = new SqlParameter("@SalerId", searchModel.SalerId);
                objParam[5] = new SqlParameter("@PageIndex", searchModel.PageIndex);
                objParam[6] = new SqlParameter("@PageSize", searchModel.PageSize);
                objParam[7] = new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                return DbWorker.GetDataTable(StoreProcedureConstant.SP_Report_TotalRevenueOrderByClient, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("TotalRevenueOrderByClient - DepartmentDAL: " + ex);
            }
            return null;
        }
        public async Task<Department> GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return await _DbContext.Departments.AsNoTracking().FirstOrDefaultAsync(s => id == s.Id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByIds - UserDAL: " + ex);
                return null;
            }
        }
    }
}
