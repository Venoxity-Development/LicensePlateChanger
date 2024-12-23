using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LicensePlateChanger.Engine.Data
{
    public class VehicleClassOptions
    {
        public bool isEnabled { get; set; }
        public List<PlateSet> plateSets { get; set; }
    }

    public class VehicleTypeOptions
    {
        public bool isTypeEnabled { get; set; }
        public string className { get; set; }
        public List<PlateSet> plateSets { get; set; }
        public List<string> allowedVehicles { get; set; }
    }

    public class VehicleData
    {
        [DataMember(Name = "vehicleClass")]
        public Dictionary<VehicleClass, VehicleClassOptions> VehicleClassOptions { get; set; }

        [DataMember(Name = "vehicleType")]
        public Dictionary<string, VehicleTypeOptions> VehicleTypeOptions { get; set; }
    }
}
