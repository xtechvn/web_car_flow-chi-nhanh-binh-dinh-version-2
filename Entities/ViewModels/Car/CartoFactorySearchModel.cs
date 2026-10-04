using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels.Car
{
    public class CartoFactorySearchModel
    {
        public string? VehicleNumber { get; set; }        // Biển số xe
        public string? PhoneNumber { get; set; }          // Số điện thoại
        public string? VehicleStatus { get; set; }        // Trạng thái xe đến
        public string? LoadType { get; set; }             // Loại xanh/thuong
        public string? VehicleWeighingType { get; set; }  // Trạng thái gọi xe vào cân
        public string? VehicleTroughStatus { get; set; }  // Trạng thái  xe đã vào máng
        public string? TroughType { get; set; }           // Loại máng
        public string? VehicleWeighingStatus { get; set; } // Trạng thái  xe đã cân ra
        public int? type { get; set; }                    // Loại 1 đã sử lý,0 chưa sl
        public int? LoadingStatus { get; set; }           // Trạng sử lý đang tải
        public int? VehicleWeighedstatus { get; set; }    // Trạng thái xe đã được cân đầu vào
        public DateTime? RegistrationTime { get; set; } // Thời gian đăng ký

    }
    public class RegistrationRecord
    {
        public string _id { get; set; }
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string GPLX { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string ZaloStatus { get; set; } = string.Empty;
        public string Camp { get; set; } = string.Empty;
        public int Type { get; set; } = 0;
        public string CreateTime { get; set; }
        public int Bookingid { get; set; }
        public string text_voice { get; set; }
        public string AudioPath { get; set; }
        public int TrangThai { get; set; }

    }
    public class SummaryReportSearchModel
    {
        public int LoadType { get; set; }             // Loại xanh/thuong
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    
    }  
    public class CamModel
    {
        public string bien_so { get; set; }             
        public string thoi_gian_chup { get; set; }            
        public string anh_chup_xe { get; set; }           
      
    
    }
}
