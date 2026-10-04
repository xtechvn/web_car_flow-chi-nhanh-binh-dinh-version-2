using Aspose.Cells;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Car;
using Microsoft.Extensions.Options;
using Nest;
using Repositories.IRepositories;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class VehicleInspectionRepository : IVehicleInspectionRepository
    {
        private readonly VehicleInspectionDAL _VehicleInspectionDAL;
        private readonly AllCodeDAL _AllCodeDAL;
        public VehicleInspectionRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _VehicleInspectionDAL = new VehicleInspectionDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _AllCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        //danh sách xe đăng ký
        public async Task<List<CartoFactoryModel>> GetListRegisteredVehicle(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);

                searchModel.RegistrationTime = expireAt;

                return await _VehicleInspectionDAL.GetListRegisteredVehicle(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListRegisteredVehicle - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListCartoFactory(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);

                searchModel.RegistrationTime = expireAt;

                return await _VehicleInspectionDAL.GetListCartoFactory(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListCartoFactory - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<int> UpdateCar(VehicleInspectionUpdateModel model)
        {
            try
            {
                return await _VehicleInspectionDAL.UpdateVehicleInspection(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateVehicleInspection - VehicleInspectionRepository: " + ex);
                return -1;
            }
        }

        public async Task<CartoFactoryModel> GetDetailtVehicleInspection(int id)
        {
            try
            {
                return await _VehicleInspectionDAL.GetDetailtVehicleInspection(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListCartoFactory - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public int SaveVehicleInspection(RegistrationRecord model)
        {
            try
            {
                return _VehicleInspectionDAL.SaveVehicleInspection(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SaveVehicleInspection - VehicleInspectionRepository: " + ex);
            }
            return 0;
        }
        public Task<string> GetAudioPathByVehicleNumber(string VehicleNumber)
        {
            try
            {
                return _VehicleInspectionDAL.GetAudioPathByVehicleNumber(VehicleNumber);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SaveVehicleInspection - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListVehicleInspectionSynthetic(DateTime? FromDate, DateTime? ToDate, int LoadType)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                if (ToDate != null)
                {
                    ToDate = ((DateTime)ToDate).Date.AddHours(hours).AddMinutes(minutes).AddSeconds(0);
                }
                else
                {
                    ToDate = expireAt;
                }


                return await _VehicleInspectionDAL.GetListVehicleInspectionSynthetic(FromDate, ToDate, LoadType);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleInspectionSynthetic - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<TotalVehicleInspection> CountTotalVehicleInspectionSynthetic(DateTime? FromDate, DateTime? ToDate)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                if (ToDate != null)
                {
                    ToDate = ((DateTime)ToDate).Date.AddHours(hours).AddMinutes(minutes).AddSeconds(0);
                }
                else
                {
                    ToDate = expireAt;
                }
                return await _VehicleInspectionDAL.CountTotalVehicleInspectionSynthetic(FromDate, ToDate);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleInspectionSynthetic - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<TotalWeightByHourModel>> GetTotalWeightByHour(DateTime? RegistrationTime)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetTotalWeightByHour(RegistrationTime);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByHour - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<TotalWeightByHourModel>> GetTotalWeightByWeightGroup(DateTime? RegistrationTime)
        {
            try
            {

                return await _VehicleInspectionDAL.GetTotalWeightByWeightGroup(RegistrationTime);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByHour - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<TotalWeightByHourModel>> GetTotalWeightByTroughType(DateTime? RegistrationTime)
        {
            try
            {

                return await _VehicleInspectionDAL.GetTotalWeightByTroughType(RegistrationTime);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalWeightByTroughType - VehicleInspectionRepository: " + ex);
            }
            return null;
        }

        public async Task<List<CartoFactoryModel>> GetListVehicleProcessingIsLoading(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                searchModel.RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetListVehicleProcessingIsLoading(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleProcessingIsLoading - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListVehicleWeighedInput(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                searchModel.RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetListVehicleWeighedInput(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleWeighedInput - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListVehicleCarCallList(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                searchModel.RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetListVehicleCarCallList(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleCarCallList - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListVehicleCallTheScale(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                searchModel.RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetListVehicleCallTheScale(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleCallTheScale - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<CartoFactoryModel>> GetListVehicleListVehicles(CartoFactorySearchModel searchModel)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                searchModel.RegistrationTime = expireAt;
                return await _VehicleInspectionDAL.GetListVehicleListVehicles(searchModel);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleListVehicles - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<List<TroughWeight>> GetListTroughWeightByVehicleInspectionId(int id)
        {
            try
            {

                return await _VehicleInspectionDAL.GetListTroughWeightByVehicleInspectionId(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTroughWeightByVehicleInspectionId - VehicleInspectionRepository: " + ex);
            }
            return new List<TroughWeight>();
        }
        public async Task<int> InsertTroughWeight(TroughWeight model)
        {
            try
            {

                return await _VehicleInspectionDAL.InsertTroughWeight(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTroughWeightByVehicleInspectionId - VehicleInspectionRepository: " + ex);
            }
            return -1;
        }
        public async Task<int> UpdateTroughWeight(TroughWeight model)
        {
            try
            {

                return await _VehicleInspectionDAL.UpdateTroughWeight(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTroughWeightByVehicleInspectionId - VehicleInspectionRepository: " + ex);
            }
            return -1;
        }
        public async Task<TroughWeight> GetDetailTroughWeightById(int id)
        {
            try
            {

                return await _VehicleInspectionDAL.GetDetailTroughWeightById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailTroughWeightById - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
        public async Task<int> UpdateVehicleInspectionByVehicleNumber(string VehicleNumber)
        {
            try
            {
                return await _VehicleInspectionDAL.UpdateVehicleInspectionByVehicleNumber(VehicleNumber);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SaveVehicleInspection - VehicleInspectionRepository: " + ex);
            }
            return 0;
        }
        public async Task<int> UpdateVehicleLoadTaken(int Id, int VehicleLoadTaken)
        {
            try
            {
                return await _VehicleInspectionDAL.UpdateVehicleLoadTaken(Id, VehicleLoadTaken);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateVehicleLoadTaken - VehicleInspectionRepository: " + ex);
            }
            return 0;
        }
        public async Task<string> ExportSummaryReport(List<CartoFactoryModel> data, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách xe";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 19);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);
                    cell.SetColumnWidth(13, 25);
                    cell.SetColumnWidth(14, 25);
                    cell.SetColumnWidth(15, 25);
                    cell.SetColumnWidth(16, 25);
                    cell.SetColumnWidth(17, 25);
                    cell.SetColumnWidth(18, 25);
                    cell.SetColumnWidth(19, 25);
                  



                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Giờ đăng ký online");
                    ws.Cells["C1"].PutValue("Tên khách hàng (Trại, đại lý)");
                    ws.Cells["D1"].PutValue("Mã khách hàng (Trại, đại lý)");
                    ws.Cells["E1"].PutValue("Tên lái xe");
                    ws.Cells["F1"].PutValue("Số điện thoại");
                    ws.Cells["G1"].PutValue("Biển số xe");
                    ws.Cells["H1"].PutValue("Nhóm tài");
                    ws.Cells["I1"].PutValue("Giờ xe có mặt");
                    ws.Cells["J1"].PutValue("Giờ vào cân");
                    ws.Cells["K1"].PutValue("Giờ cân xong đầu vào");
                    ws.Cells["L1"].PutValue("Máng");
                    ws.Cells["M1"].PutValue("Giờ vào máng");
                    ws.Cells["N1"].PutValue("Giờ ra máng");
                    ws.Cells["O1"].PutValue("Giờ cân xong đầu ra");
                    ws.Cells["P1"].PutValue("Thời gian xuất hàng(Phút)");
                    ws.Cells["Q1"].PutValue("Tình trạng");
                    ws.Cells["R1"].PutValue("Tổng trọng lượng đã lấy(KG)");
                    ws.Cells["S1"].PutValue("CSOS");
                    ws.Cells["T1"].PutValue("Cân vào");
                    ws.Cells["U1"].PutValue("Cân ra");
                    ws.Cells["V1"].PutValue("NET WEIGHT");


                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count * 2, 19);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["A2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in data)
                    {
                        var customers = SplitCustomer(item.CustomerName);

                        int startRow = RowIndex + 1; // dòng bắt đầu
                        int totalRow = customers.Count;

                        bool isFirstRow = true;
                        var TrangThai_name = "";
                        // Ưu tiên kiểm tra từ Bước 6 xuống Bước 1 
                        if((item.VehicleTroughStatus == (int)VehicleTroughStatus.Hoan_thanh) && (item.VehicleWeighingStatus != null))
                        {
                            TrangThai_name = "Bước 6";
                        }
                        else if ((item.VehicleWeighedstatus == (int)VehicleWeighedstatus.Da_Can_Xong_Dau_Cao) && item.TroughTypes != null &&
                        (item.VehicleTroughStatus != null || item.VehicleTroughStatus == (int)VehicleTroughStatus.Blank))
                        {
                            TrangThai_name = "Bước 5";
                        }
                        else if ((item.VehicleWeighingType == (int)VehicleWeighingType.DA_Vao_Can) &&
                        (item.VehicleWeighedstatus != null || item.VehicleWeighedstatus == (int)VehicleWeighedstatus.Blank))
                        {
                            TrangThai_name = "Bước 4";
                        }
                        else if ((item.LoadingStatus == (int)LoadingStatus.Da_HTTC) &&
                        (item.VehicleWeighingType != null || item.VehicleWeighingType == (int)VehicleWeighingType.Blank))
                        {
                            TrangThai_name = "Bước 3";
                        }
                        else if ((item.VehicleStatus == (int)VehicleStatus.Da_Den_NM) &&
                        (item.LoadingStatus != null || item.LoadingStatus == (int)LoadingStatus.Blank))
                        {
                            TrangThai_name = "Bước 2";
                        }
                        else if (item.VehicleStatus != null && (item.LoadingStatus == null || item.LoadingStatus == (int)VehicleWeighingType.Blank))
                        {
                            TrangThai_name = "Bước 1";
                        }

                      
                        string ttchitiet = string.Empty;

                        foreach (var cus in customers)
                        {
                            RowIndex++;

                            if (isFirstRow)
                            {
                                ws.Cells["A" + RowIndex].PutValue(item.RecordNumber);
                                ws.Cells["B" + RowIndex].PutValue(((DateTime)item.RegisterDateOnline).ToString("dd/MM/yyyy HH:mm"));
                                ws.Cells["E" + RowIndex].PutValue(item.DriverName);
                                ws.Cells["F" + RowIndex].PutValue(item.PhoneNumber);
                                ws.Cells["G" + RowIndex].PutValue(item.VehicleNumber);
                                ws.Cells["H" + RowIndex].PutValue(item.LoadTypeName);
                                ws.Cells["I" + RowIndex].PutValue(item.VehicleArrivalDate != null ? item.VehicleArrivalDate.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["J" + RowIndex].PutValue(item.VehicleWeighingTimeComeIn != null ? item.VehicleWeighingTimeComeIn.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["K" + RowIndex].PutValue(item.VehicleWeighingTimeComeOut != null ? item.VehicleWeighingTimeComeOut.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["L" + RowIndex].PutValue(item.TroughTypes);
                                ws.Cells["M" + RowIndex].PutValue(item.VehicleTroughTimeComeIn != null ? item.VehicleTroughTimeComeIn.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["N" + RowIndex].PutValue(item.VehicleTroughTimeComeOut != null ? item.VehicleTroughTimeComeOut.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["O" + RowIndex].PutValue(item.VehicleWeighingTimeComplete != null ? item.VehicleWeighingTimeComplete.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["P" + RowIndex].PutValue(item.totalMinutes);
                                ws.Cells["Q" + RowIndex].PutValue(TrangThai_name);
                                ws.Cells["R" + RowIndex].PutValue(item.TotalVehicleTroughWeight);
                                ws.Cells["S" + RowIndex].PutValue(item.FullName);
                                ws.Cells["T" + RowIndex].PutValue(item.VehicleWeightIn);
                                ws.Cells["U" + RowIndex].PutValue(item.VehicleWeightOut);
                                ws.Cells["V" + RowIndex].PutValue(item.NetWeight);
                         

                                isFirstRow = false;
                            }

                            ws.Cells["C" + RowIndex].PutValue(cus.Name);
                            ws.Cells["D" + RowIndex].PutValue(cus.Code);
                        }
                        // 👉 MERGE CELL
                        if (totalRow > 1)
                        {
                            ws.Cells.Merge(startRow - 1, 0, totalRow, 1); // A
                            ws.Cells.Merge(startRow - 1, 1, totalRow, 1); // B
                            ws.Cells.Merge(startRow - 1, 4, totalRow, 1); // E
                            ws.Cells.Merge(startRow - 1, 5, totalRow, 1); // F
                            ws.Cells.Merge(startRow - 1, 6, totalRow, 1); // G
                            ws.Cells.Merge(startRow - 1, 7, totalRow, 1); // H
                            ws.Cells.Merge(startRow - 1, 8, totalRow, 1); // I
                            ws.Cells.Merge(startRow - 1, 9, totalRow, 1); // J
                            ws.Cells.Merge(startRow - 1, 10, totalRow, 1); // K
                            ws.Cells.Merge(startRow - 1, 11, totalRow, 1); // L
                            ws.Cells.Merge(startRow - 1, 12, totalRow, 1); // M
                            ws.Cells.Merge(startRow - 1, 13, totalRow, 1); // N
                            ws.Cells.Merge(startRow - 1, 14, totalRow, 1); // O
                            ws.Cells.Merge(startRow - 1, 15, totalRow, 1); // P
                            ws.Cells.Merge(startRow - 1, 16, totalRow, 1); // Q
                            ws.Cells.Merge(startRow - 1, 17, totalRow, 1); // R
                            ws.Cells.Merge(startRow - 1, 18, totalRow, 1); // S
                        }
                        RowIndex++;

                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDeposit - OrderRepository: " + ex);
            }
            return pathResult;
        } 
        public async Task<string> ExportDeposit(CartoFactorySearchModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {

                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);

                searchModel.RegistrationTime = expireAt;

                var data = await _VehicleInspectionDAL.GetListCartoFactory_EX(searchModel);
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách xe đăng ký";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 14);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);
                    cell.SetColumnWidth(13, 25);



                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Giờ đăng ký online");
                    ws.Cells["C1"].PutValue("Tên khách hàng (Trại, đại lý)");
                    ws.Cells["D1"].PutValue("Mã khách hàng (Trại, đại lý)");
                    ws.Cells["E1"].PutValue("Tên lái xe");
                    ws.Cells["F1"].PutValue("Số điện thoại");
                    ws.Cells["G1"].PutValue("Biển số xe");
                    ws.Cells["H1"].PutValue("Tải trọng xe (kg)");
                    ws.Cells["I1"].PutValue("KH Ghi chú");
                    ws.Cells["J1"].PutValue("BV ghi chú");
                    ws.Cells["K1"].PutValue("TT đăng kiểm");
                    ws.Cells["L1"].PutValue("Tình trạng xe");


                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count * 2, 14);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["A2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in data)
                    {
                        var customers = SplitCustomer(item.CustomerName);

                        int startRow = RowIndex + 1; // dòng bắt đầu
                        int totalRow = customers.Count;

                        bool isFirstRow = true;
                        var TrangThai_name = "";
                        switch (item.TrangThai)
                        {
                            case 1:
                                TrangThai_name = "Chưa có";
                                break;
                            case 2:
                                TrangThai_name = "Còn hạn";
                                break;
                            case 3:
                                TrangThai_name = "Sắp hết hạn";
                                break;
                            case 4:
                                TrangThai_name = "Hết hạn";
                                break;

                        }
                        string ttchitiet = string.Empty;

                        foreach (var cus in customers)
                        {
                            RowIndex++;

                            if (isFirstRow)
                            {
                                ws.Cells["A" + RowIndex].PutValue(item.RecordNumber);
                                ws.Cells["B" + RowIndex].PutValue(((DateTime)item.RegisterDateOnline).ToString("dd/MM/yyyy HH:mm"));
                                ws.Cells["E" + RowIndex].PutValue(item.DriverName);
                                ws.Cells["F" + RowIndex].PutValue(item.PhoneNumber);
                                ws.Cells["G" + RowIndex].PutValue(item.VehicleNumber);
                                ws.Cells["H" + RowIndex].PutValue(item.VehicleLoad);
                                ws.Cells["I" + RowIndex].PutValue(item.LicenseNumber);
                                ws.Cells["J" + RowIndex].PutValue(item.ProtectNotes);
                                ws.Cells["K" + RowIndex].PutValue(TrangThai_name);
                                ws.Cells["L" + RowIndex].PutValue(item.VehicleStatusName);

                                isFirstRow = false;
                            }

                            ws.Cells["C" + RowIndex].PutValue(cus.Name);
                            ws.Cells["D" + RowIndex].PutValue(cus.Code);
                        }
                        // 👉 MERGE CELL
                        if (totalRow > 1)
                        {
                            ws.Cells.Merge(startRow - 1, 0, totalRow, 1); // A
                            ws.Cells.Merge(startRow - 1, 1, totalRow, 1); // B
                            ws.Cells.Merge(startRow - 1, 4, totalRow, 1); // E
                            ws.Cells.Merge(startRow - 1, 5, totalRow, 1); // F
                            ws.Cells.Merge(startRow - 1, 6, totalRow, 1); // G
                            ws.Cells.Merge(startRow - 1, 7, totalRow, 1); // H
                            ws.Cells.Merge(startRow - 1, 8, totalRow, 1); // I
                            ws.Cells.Merge(startRow - 1, 9, totalRow, 1); // J
                            ws.Cells.Merge(startRow - 1, 10, totalRow, 1); // K
                            ws.Cells.Merge(startRow - 1, 11, totalRow, 1); // L
                        }
                        RowIndex++;

                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDeposit - OrderRepository: " + ex);
            }
            return pathResult;
        }
        public List<(string Name, string Code)> SplitCustomer(string input)
        {
            var result = new List<(string, string)>();

            if (string.IsNullOrWhiteSpace(input))
                return result;

            var lines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var text = line.Trim();

                // tìm dấu - cuối cùng
                int lastDashIndex = text.LastIndexOf('-');

                if (lastDashIndex > 0)
                {
                    var name = text.Substring(0, lastDashIndex).Trim();
                    var code = text.Substring(lastDashIndex + 1).Trim();

                    result.Add((name, code));
                }
                else
                {
                    result.Add((text, ""));
                }
            }

            return result;
        }
        public async Task<string> ExportTimeProductDelivery(List<VehicleInspectionTimeProductDeliveryModel> data, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách xe";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 19);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);
                    cell.SetColumnWidth(13, 25);
                    cell.SetColumnWidth(14, 25);
                    cell.SetColumnWidth(15, 25);
                    cell.SetColumnWidth(16, 25);
                    cell.SetColumnWidth(17, 25);
                    cell.SetColumnWidth(18, 25);
                    cell.SetColumnWidth(19, 25);
                    cell.SetColumnWidth(20, 25);
                    cell.SetColumnWidth(21, 25);




                    // Set header value
                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Giờ đăng ký online");
                    ws.Cells["C1"].PutValue("Giờ đăng tài");
                    ws.Cells["D1"].PutValue("Biển số xe");
                    ws.Cells["E1"].PutValue("Tên Khách hàng");
                    ws.Cells["F1"].PutValue("Mã Khách hàng");
                    ws.Cells["G1"].PutValue("Tl hàng cho phép");
                    ws.Cells["H1"].PutValue("Máng xuất");
                    ws.Cells["I1"].PutValue("Xe nâng");
                    ws.Cells["J1"].PutValue("Thời gian xe vào máng");
                    ws.Cells["K1"].PutValue("Thời gian xe ra khỏi máng");
                    ws.Cells["L1"].PutValue("Cân vào");
                    ws.Cells["M1"].PutValue("Cân ra");
                    ws.Cells["N1"].PutValue("NET WEIGHT");
                    ws.Cells["O1"].PutValue(" T.Lượng đơn hàng");
                    ws.Cells["P1"].PutValue("số bao");
                    ws.Cells["Q1"].PutValue("Trừ bì");
                    ws.Cells["R1"].PutValue("Lệch");
                    ws.Cells["S1"].PutValue("LIMIT");
                    ws.Cells["T1"].PutValue("Duyệt");
                    ws.Cells["U1"].PutValue("Giờ cân vào");
                    ws.Cells["V1"].PutValue("Giờ cân ra");


                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count * 2, 21);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["A2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 1;

                    foreach (var item in data)
                    {
                        var customers = SplitCustomer(item.CustomerName);

                        int startRow = RowIndex + 1; // dòng bắt đầu
                        int totalRow = customers.Count;

                        bool isFirstRow = true;
                        var TrangThai_name = "";
  


                        string ttchitiet = string.Empty;

                        foreach (var cus in customers)
                        {
                            RowIndex++;

                            if (isFirstRow)
                            {
                                ws.Cells["A" + RowIndex].PutValue(item.RecordNumber);
                                ws.Cells["B" + RowIndex].PutValue(item.RegisterDateOnline!=null?((DateTime)item.RegisterDateOnline).ToString("HH:mm dd/MM/yyyy"):"");
                                ws.Cells["C" + RowIndex].PutValue(item.VehicleArrivalDate!=null?((DateTime)item.VehicleArrivalDate).ToString("HH:mm dd/MM/yyyy"):"");
                                ws.Cells["D" + RowIndex].PutValue(item.VehicleNumber);

                                ws.Cells["G" + RowIndex].PutValue(item.VehicleWeightMax);
                                ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["H" + RowIndex].PutValue(item.TroughTypes);
                                ws.Cells["I" + RowIndex].PutValue("");
                                ws.Cells["J" + RowIndex].PutValue(item.VehicleWeighingTimeComeIn != null ? item.VehicleWeighingTimeComeIn.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["K" + RowIndex].PutValue(item.VehicleWeighingTimeComeOut != null ? item.VehicleWeighingTimeComeOut.Value.ToString("HH:mm dd/MM/yyyy") : "");
                               
                                ws.Cells["L" + RowIndex].PutValue(item.VehicleWeightIn != null ? item.VehicleWeightIn : 0);
                                ws.Cells["L" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["M" + RowIndex].PutValue(item.VehicleWeightOut != null ? item.VehicleWeightOut : 0);
                                ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["N" + RowIndex].PutValue((item.NetWeight != null ? item.NetWeight.Value : 0).ToString("N0"));
                                ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["O" + RowIndex].PutValue((item.OrderWeight != null ? item.OrderWeight.Value : 0).ToString("N2"));
                                ws.Cells["O" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["P" + RowIndex].PutValue((item.TotalBag != null ? item.TotalBag.Value : 0).ToString("N2"));
                                ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["Q" + RowIndex].PutValue((item.DeductionWeight != null ? item.DeductionWeight.Value : 0).ToString("N2"));
                                ws.Cells["Q" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["R" + RowIndex].PutValue((item.DifferenceWeight != null ? item.DifferenceWeight.Value : 0).ToString("N0"));
                                ws.Cells["R" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["S" + RowIndex].PutValue(item.LimitValue);
                                ws.Cells["S" + RowIndex].SetStyle(numberStyle);
                                ws.Cells["T" + RowIndex].PutValue(item.ApproveStatus);
                                ws.Cells["U" + RowIndex].PutValue(item.VehicleTroughTimeComeOut != null ? item.VehicleTroughTimeComeOut.Value.ToString("HH:mm dd/MM/yyyy") : "");
                                ws.Cells["V" + RowIndex].PutValue(item.VehicleWeighingTimeComplete != null ? item.VehicleWeighingTimeComplete.Value.ToString("HH:mm dd/MM/yyyy") : "");


                                isFirstRow = false;
                            }

                            ws.Cells["E" + RowIndex].PutValue(cus.Name);
                            ws.Cells["F" + RowIndex].PutValue(cus.Code);
                        }
                        // 👉 MERGE CELL
                        if (totalRow > 1)
                        {
                            ws.Cells.Merge(startRow - 1, 0, totalRow, 1); // A
                            ws.Cells.Merge(startRow - 1, 1, totalRow, 1); // B
                            ws.Cells.Merge(startRow - 1, 2, totalRow, 1); // C
                            ws.Cells.Merge(startRow - 1, 3, totalRow, 1); // D
                            ws.Cells.Merge(startRow - 1, 6, totalRow, 1); // G
                            ws.Cells.Merge(startRow - 1, 7, totalRow, 1); // H
                            ws.Cells.Merge(startRow - 1, 8, totalRow, 1); // I
                            ws.Cells.Merge(startRow - 1, 9, totalRow, 1); // J
                            ws.Cells.Merge(startRow - 1, 10, totalRow, 1); // K
                            ws.Cells.Merge(startRow - 1, 11, totalRow, 1); // L
                            ws.Cells.Merge(startRow - 1, 12, totalRow, 1); // M
                            ws.Cells.Merge(startRow - 1, 13, totalRow, 1); // N
                            ws.Cells.Merge(startRow - 1, 14, totalRow, 1); // O
                            ws.Cells.Merge(startRow - 1, 15, totalRow, 1); // P
                            ws.Cells.Merge(startRow - 1, 16, totalRow, 1); // Q
                            ws.Cells.Merge(startRow - 1, 17, totalRow, 1); // R
                            ws.Cells.Merge(startRow - 1, 18, totalRow, 1); // S
                            ws.Cells.Merge(startRow - 1, 19, totalRow, 1); // T
                            ws.Cells.Merge(startRow - 1, 20, totalRow, 1); // U
                            ws.Cells.Merge(startRow - 1, 21, totalRow, 1); // V
                        }
                        RowIndex++;

                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDeposit - OrderRepository: " + ex);
            }
            return pathResult;
        }
        public async Task<List<VehicleInspectionTimeProductDeliveryModel>> GetListVehicleInspectionTimeProductDelivery(DateTime? FromDate, DateTime? ToDate, int LoadType)
        {
            try
            {
                var TIME_RESET = await _AllCodeDAL.GetListSortByName(AllCodeType.TIME_RESET);
                var hours = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                            ? TIME_RESET[0].UpdateTime.Value.Hour
                            : 17;
                var minutes = TIME_RESET != null && TIME_RESET.Count > 0 && TIME_RESET[0].UpdateTime.HasValue
                              ? TIME_RESET[0].UpdateTime.Value.Minute
                              : 55;
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
                if (ToDate != null)
                {
                    ToDate = ((DateTime)ToDate).Date.AddHours(hours).AddMinutes(minutes).AddSeconds(0);
                }
                else
                {
                    ToDate = expireAt;
                }


                return await _VehicleInspectionDAL.GetListVehicleInspectionTimeProductDelivery(FromDate, ToDate, LoadType);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListVehicleInspectionSynthetic - VehicleInspectionRepository: " + ex);
            }
            return null;
        }
    }
}
