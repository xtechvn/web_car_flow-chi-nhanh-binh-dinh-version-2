using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TroughWeight
    {
        public int Id { get; set; }
        public int? VehicleInspectionId { get; set; }
        public int? TroughType { get; set; }
        public decimal? VehicleTroughWeight { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdateBy { get; set; }
    }
}
