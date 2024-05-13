using System.Collections.Generic;

namespace LicensePlateChanger.Models
{
    public class vehicleClass
    {
        public bool isEnabled { get; set; }
    }

    public class vehicleData
    {
        public Dictionary<string, vehicleClass> vehicleClass { get; set; }
    }
}