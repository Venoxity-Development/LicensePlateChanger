using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LicensePlateChanger.Models
{
    public class VehicleClassOptions
    {
        public bool isEnabled { get; set; }
    }

    public class VehicleData
    {
        [DataMember(Name = "vehicleClass")]
        public Dictionary<string, VehicleClassOptions> VehicleClassOptions { get; set; }
    }
}