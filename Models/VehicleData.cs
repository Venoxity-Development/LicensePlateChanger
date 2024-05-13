namespace LicensePlateChanger.Models
{
    public class VehicleClass
    {
        public bool isEnabled { get; set; }
    }

    public class VehicleData
    {
        public VehicleClasses VehicleClass { get; set; }
    }
}