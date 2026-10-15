using DAL;
using Entities.ConfigModels;
using Entities.ViewModels.ObdConfig;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class ObdConfigRepository : IObdConfigRepository
    {
        public const int IMPORT_MAX_ROWS = 10000;
        private const int MAX_ERRORS_SHOWN = 50;
        private const string HEADER_DELIVERY = "delivery";
        private const string HEADER_NET_WEIGHT = "netweight";

        private readonly ObdConfigDAL _obdConfigDAL;

        public ObdConfigRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _obdConfigDAL = new ObdConfigDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<DateTime> GetBusinessDate()
        {
            var date = await _obdConfigDAL.GetBusinessDate();
            return date ?? DateTime.Today;
        }

        public ObdConfigImportResult ReadImportFile(Stream stream)
        {
            var result = new ObdConfigImportResult();
            try
            {
                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (sheet == null || sheet.Dimension == null)
                    {
                        result.Errors.Add("File Excel không có dữ liệu.");
                        return result;
                    }

                    int lastRow = sheet.Dimension.End.Row;
                    int lastCol = sheet.Dimension.End.Column;
                    int colDelivery = 0, colNetWeight = 0;

                    for (int col = 1; col <= lastCol; col++)
                    {
                        var header = (sheet.Cells[1, col].Text ?? "").Replace(" ", "").Trim().ToLowerInvariant();
                        if (header == HEADER_DELIVERY && colDelivery == 0) colDelivery = col;
                        else if (header == HEADER_NET_WEIGHT && colNetWeight == 0) colNetWeight = col;
                    }

                    if (colDelivery == 0 || colNetWeight == 0)
                    {
                        var missing = new List<string>();
                        if (colDelivery == 0) missing.Add("Delivery");
                        if (colNetWeight == 0) missing.Add("Net Weight");
                        result.Errors.Add($"Không tìm thấy cột {string.Join(", ", missing)} ở hàng đầu tiên. "
                            + "Hàng 1 phải là tên cột gồm \"Delivery\" và \"Net Weight\", dữ liệu bắt đầu từ hàng 2.");
                        return result;
                    }

                    int errorCount = 0;
                    for (int row = 2; row <= lastRow; row++)
                    {
                        var deliveryValue = sheet.Cells[row, colDelivery].Value;
                        var netWeightValue = sheet.Cells[row, colNetWeight].Value;
                        bool deliveryEmpty = deliveryValue == null || string.IsNullOrWhiteSpace(deliveryValue.ToString());
                        bool netWeightEmpty = netWeightValue == null || string.IsNullOrWhiteSpace(netWeightValue.ToString());
                        if (deliveryEmpty && netWeightEmpty) continue;

                        var rowErrors = new List<string>();
                        long delivery = 0;
                        decimal netWeight = 0;

                        if (deliveryEmpty)
                            rowErrors.Add("Delivery đang trống");
                        else if (!ObdValueParser.TryParseDelivery(deliveryValue, out delivery))
                            rowErrors.Add($"Delivery \"{deliveryValue}\" không hợp lệ (chỉ gồm chữ số, tối đa {ObdValueParser.DELIVERY_MAX_LENGTH} số)");

                        if (netWeightEmpty)
                            rowErrors.Add("Net Weight đang trống");
                        else if (!ObdValueParser.TryParseNetWeight(netWeightValue, out netWeight))
                            rowErrors.Add($"Net Weight \"{netWeightValue}\" không hợp lệ (phải là số không âm)");

                        if (rowErrors.Count > 0)
                        {
                            errorCount++;
                            if (errorCount <= MAX_ERRORS_SHOWN)
                                result.Errors.Add($"Dòng {row}: {string.Join("; ", rowErrors)}.");
                            continue;
                        }

                        result.Rows.Add(new ObdConfigImportRow { RowNumber = row, Delivery = delivery, NetWeight = netWeight });
                    }

                    if (errorCount > MAX_ERRORS_SHOWN)
                        result.Errors.Add($"... và {errorCount - MAX_ERRORS_SHOWN} dòng lỗi khác.");

                    int totalRows = result.Rows.Count + errorCount;
                    if (totalRows == 0)
                        result.Errors.Add("File Excel không có dòng dữ liệu nào (dữ liệu phải bắt đầu từ hàng 2).");
                    else if (totalRows > IMPORT_MAX_ROWS)
                        result.Errors.Insert(0, $"File có {totalRows:N0} dòng dữ liệu, vượt quá giới hạn {IMPORT_MAX_ROWS:N0} dòng. Vui lòng tách thành nhiều file.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ReadImportFile - ObdConfigRepository: " + ex);
                result.Errors.Add("Không đọc được file. Vui lòng kiểm tra file đúng định dạng Excel .xlsx và không bị khóa mật khẩu.");
            }
            return result;
        }

        public async Task<ObdConfigSaveResult> InsertFromExcel(List<ObdConfigImportRow> rows, int userId, string fileName)
        {
            try
            {
                return await _obdConfigDAL.InsertFromExcel(rows, userId, fileName);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertFromExcel - ObdConfigRepository: " + ex);
            }
            return null;
        }

        public async Task<ObdConfigSaveResult> Insert(long delivery, decimal netWeight, int userId)
        {
            try
            {
                return await _obdConfigDAL.Insert(delivery, netWeight, userId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - ObdConfigRepository: " + ex);
            }
            return null;
        }

        public async Task<ObdConfigSaveResult> UpdateDelivery(long obdId, long delivery, int userId)
        {
            try
            {
                return await _obdConfigDAL.UpdateDelivery(obdId, delivery, userId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateDelivery - ObdConfigRepository: " + ex);
            }
            return null;
        }

        public async Task<List<ObdConfigVersionModel>> GetListVersion(DateTime loadingDate)
        {
            try
            {
                return await _obdConfigDAL.GetListVersion(loadingDate);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVersion - ObdConfigRepository: " + ex);
            }
            return new List<ObdConfigVersionModel>();
        }

        public async Task<List<ObdConfigModel>> GetList(DateTime loadingDate, int version, string delivery, int pageIndex, int pageSize)
        {
            try
            {
                return await _obdConfigDAL.GetList(loadingDate, version, delivery, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - ObdConfigRepository: " + ex);
            }
            return new List<ObdConfigModel>();
        }

        public ObdConfigHistoryModel GetHistory(long obdId)
        {
            try
            {
                var detail = _obdConfigDAL.GetById(obdId);
                if (detail == null) return null;
                return new ObdConfigHistoryModel
                {
                    Detail = detail,
                    Logs = _obdConfigDAL.GetListLog(obdId)
                };
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetHistory - ObdConfigRepository: " + ex);
            }
            return null;
        }
    }
}
