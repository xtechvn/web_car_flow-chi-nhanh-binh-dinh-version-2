using System;

namespace Entities.ViewModels.Car
{
    public class VehicleInspectionObdModel
    {
        public long Id { get; set; }
        public int VehicleInspectionId { get; set; }
        public long ObdId { get; set; }
        public long Delivery { get; set; }
        public decimal NetWeight { get; set; }
        public int? UserCreateID { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LoadingDate { get; set; }
        public int? Version { get; set; }
        public string DeliveryText => Delivery.ToString();
        public string NetWeightText => NetWeight.ToString("#,##0.###", System.Globalization.CultureInfo.InvariantCulture);
        public string LoadingDateText => LoadingDate.HasValue ? LoadingDate.Value.ToString("dd/MM/yyyy") : "";
    }

    public class ObdSuggestItem
    {
        public long ObdId { get; set; }
        public long Delivery { get; set; }
        public decimal NetWeight { get; set; }
        public DateTime LoadingDate { get; set; }
        public int Version { get; set; }
        public string DeliveryText => Delivery.ToString();
        public string NetWeightText => NetWeight.ToString("#,##0.###", System.Globalization.CultureInfo.InvariantCulture);
        public string LoadingDateText => LoadingDate.ToString("dd/MM/yyyy");
    }

    public class VehicleInspectionObdSaveResult
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public long Id { get; set; }
        public long Delivery { get; set; }
        public long ObdId { get; set; }
        public decimal NetWeight { get; set; }
    }
}
