using System;

namespace Entities.ViewModels.Car
{
    public class VehicleWeighInSkuModel
    {
        public long Id { get; set; }
        public int VehicleInspectionId { get; set; }
        public int SkuLine { get; set; }
        public decimal Weight { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string WeightText => Weight.ToString("#,##0.###", System.Globalization.CultureInfo.InvariantCulture);
    }
}
