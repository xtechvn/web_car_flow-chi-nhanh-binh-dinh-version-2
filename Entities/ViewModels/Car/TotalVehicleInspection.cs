using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Car
{
    public class TotalVehicleInspection
    {
        public double TotalCar { get; set; }//Tổng số xe theo ngày
        public double TotalCarCompleted { get; set; }//Tổng số xe đã lấy hàng
        public double TotalCarUnfinished { get; set; }//Tổng số xe chưa lấy hàng
        public double TotalCarArriving16h { get; set; }//Tổng số xe đến lấy hàng sau 16h
        public double TotalCarMultipleTrough { get; set; }//Tổng số xe ngắt máng
        public double TotalTroughType3 { get; set; }//Tổng số bản ghi theo từng loại máng
        public double TotalTroughType4 { get; set; }
        public double TotalTroughType5 { get; set; }
        public double TotalTroughType6 { get; set; }
        public double TotalTroughType7 { get; set; }
        public double TotalTroughType8 { get; set; }
        public double TotalWeightTroughType { get; set; }//TÍNH TỔNG TRỌNG LƯỢNG
        public double TotalWeightAfter16h { get; set; }//TÍNH TỔNG TRỌNG LƯỢNG sau 16h
        public double TotalWeightNotCompleted { get; set; }//TÍNH TỔNG TRỌNG LƯỢNG chưa xuất hàng
        public double TotalWeightTroughType3 { get; set; }
        public double TotalWeightTroughType4 { get; set; }
        public double TotalWeightTroughType5 { get; set; }
        public double TotalWeightTroughType6 { get; set; }
        public double TotalWeightTroughType7 { get; set; }
        public double TotalWeightTroughType8 { get; set; }
        public double TotalTimeInHour { get; set; }
        public double AvgTimePerCompletedCar_Hour { get; set; }
        public DateTime MaxVehicleTimeOutToday { get; set; }
        public double TotalTimeWorkInHour { get; set; }
        public double AverageProductivity { get; set; }
        public DateTime VehicleWeighingTimeComplete { get; set; }

    }
    public class TotalWeightByHourModel
    {
        public int CompletionHour { get; set; }
        public int TotalWeightInHour { get; set; }
        public int TotalCompletedCarsInHour { get; set; }
        public string WeightGroup { get; set; }
        public double TotalWeightTons { get; set; }
        public double TotalProcessMinutes { get; set; }
        public double SanLuong { get; set; }
        public double SoPhut_Tren_Tan { get; set; }
        public double SoPhut_Tren_Xe { get; set; }
        public string TroughType { get; set; }
        public double SoXe { get; set; }
        public double TongGio { get; set; }
        public double Tan_Moi_Gio { get; set; }
        public double KhungGio { get; set; }
    } 
    public class TotalWeightByHourViewModel
    {
        public Array CompletionHour { get; set; }
        public Array TotalWeightInHour { get; set; }
        public Array TotalCompletedCarsInHour { get; set; }
        public Array WeightGroup { get; set; }
        public Array TotalWeightTons { get; set; }
        public Array TotalProcessMinutes { get; set; }
        public Array SanLuong { get; set; }
        public Array SoPhut_Tren_Tan { get; set; }
        public Array SoPhut_Tren_Xe { get; set; }
        public Array SoXe { get; set; }
        public Array TongGio { get; set; }
        public Array Tan_Moi_Gio { get; set; }
        public Array TroughType { get; set; }
        public Array KhungGio { get; set; }

    }
}
