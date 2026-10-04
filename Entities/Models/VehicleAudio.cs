using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class VehicleAudio
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public string AudioPath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
