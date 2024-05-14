public enum VehicleClass
{
    Compacts = 0,
    Sedans = 1,
    SUVs = 2,
    Coupes = 3,
    Muscle = 4,
    SportsClassics = 5,
    Sports = 6,
    Super = 7,
    Motorcycles = 8,
    OffRoad = 9,
    Industrial = 10,
    Utility = 11,
    Vans = 12,
    Cycles = 13,
    Boats = 14,
    Helicopters = 15,
    Planes = 16,
    Service = 17,
    Emergency = 18,
    Military = 19,
    Commercial = 20,
    Trains = 21,
    OpenWheel = 22,
    Cars = 23
}

public static class VehicleClassMappings
{
    public static string[] classNames = { "Compacts", "Sedans", "SUVs", "Coupes", "Muscle", "SportsClassics", "Sports", "Super", "OffRoad", "Vans" };
    public static VehicleClass[] classValues = { VehicleClass.Compacts, VehicleClass.Sedans,
    VehicleClass.SUVs, VehicleClass.Coupes, VehicleClass.Muscle, VehicleClass.SportsClassics,
    VehicleClass.Sports, VehicleClass.Super, VehicleClass.OffRoad, VehicleClass.Vans };
}

public enum VehicleClassMappingValidationState
{
    Success,
    FailureNoMapping,
    FailureInvalidClassName
}
