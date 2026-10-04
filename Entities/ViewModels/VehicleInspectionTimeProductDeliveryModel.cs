using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class VehicleInspectionTimeProductDeliveryModel : VehicleInspection
    {
        public decimal VehicleWeightMax { get; set; }
        public string TroughTypes { get; set; }
        public string Forklift { get; set; }//-- Xe nâng 
        public decimal? TotalVehicleTroughWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? OrderWeight { get; set; }//T.Lương đơn hàng
        public decimal? TotalBag { get; set; }//Số bao
        public decimal? DeductionWeight { get; set; }// Trừ bì
        public decimal? DifferenceWeight { get; set; }//Lệch
        public int? LimitValue { get; set; }
        public string? ApproveStatus { get; set; }// Duyệt
    }
}
