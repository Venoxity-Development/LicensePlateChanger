using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LicensePlateChanger.Models
{
    public class VehicleClassOptions
    {
        public bool isEnabled { get; set; }
        public List<PlateSet> plateSets { get; set; }
    }

    public class VehicleData
    {
        [DataMember(Name = "vehicleClass")]
        public Dictionary<VehicleClass, VehicleClassOptions> VehicleClassOptions { get; set; }
    }
}