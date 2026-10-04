using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class InspectionSearchModel
    {
        public string VehicleNumber { get; set; }
        public int Type { get; set; } = -1;
    }
}
