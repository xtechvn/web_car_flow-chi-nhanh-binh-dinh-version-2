using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Car
{
    public class CartoFactoryModel: VehicleInspection
    {
        public string LoadTypeName { get; set; }
        public string VehicleStatusName { get; set; }
        public string VehicleWeighingTypeName { get; set; }
        public string TroughTypeName { get; set; }
        public string VehicleTroughStatusName { get; set; }
        public string VehicleWeighingStatusName { get; set; }
        public string LoadingStatusName { get; set; }
        public string VehicleWeighedstatusName { get; set; }
        public string AudioPath { get; set; }
        public string Note { get; set; }
        public string LoadingTypeName { get; set; }
        public string FullName { get; set; }
        public int TrangThai { get; set; }
        public int InspectionId { get; set; } 
        public List<TroughWeight> ListTroughWeight { get; set; }
        public decimal VehicleWeightMax { get; set; }
        public string TroughTypes { get; set; }
        public decimal? TotalVehicleTroughWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public int totalMinutes { get; set; }
        public string RankName { get; set; }
        public List<VehicleInspectionObdModel> ObdList { get; set; } = new List<VehicleInspectionObdModel>();
        public string ObdDeliveriesText { get; set; }
        public string GoodsTypeName { get; set; }
        /// <summary>Các lần cân SKU (Hàng xá). 1 xe — nhiều dòng.</summary>
        public List<VehicleWeighInSkuModel> WeighInSkuList { get; set; } = new List<VehicleWeighInSkuModel>();
        /// <summary>Σ NetWeight OBD gắn trên xe (tính ở UI/repo).</summary>
        public decimal ObdNetWeightTotal => ObdList != null ? ObdList.Sum(x => x.NetWeight) : 0m;
        /// <summary>Check tải trọng = VehicleWeightMax (đăng kiểm) − Σ OBD NetWeight.</summary>
        public decimal CheckLoadWeight => VehicleWeightMax - ObdNetWeightTotal;
        public decimal WeighInSkuTotal => WeighInSkuList != null ? WeighInSkuList.Sum(x => x.Weight) : 0m;

    }
}
