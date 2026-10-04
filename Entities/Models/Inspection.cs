using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Inspection
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal? VehicleWeight { get; set; }
        public DateTime? InspectionDate { get; set; }
        public decimal? VehicleWeightMax { get; set; }
    }
}
