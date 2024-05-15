using GTA;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LicensePlateChanger.Extensions
{
    internal static class VehicleExtensions
    {
        public static bool IsVehicleExcluded(Vehicle vehicle)
        {
            return false;
        }

        public static string GetClassNameForVehicle(Vehicle vehicle)
        {
            List<VehicleClass> allowedClasses = new List<VehicleClass> { VehicleClass.Compacts, VehicleClass.Sedans,
                VehicleClass.SUVs, VehicleClass.Coupes, VehicleClass.Muscle, VehicleClass.SportsClassics,
                VehicleClass.Sports, VehicleClass.Super, VehicleClass.OffRoad, VehicleClass.Vans };

            if (allowedClasses.Contains((VehicleClass)vehicle.ClassType))
            {
                return Configuration.VehicleClassMapping.FirstOrDefault(x => x.Value == VehicleClass.Cars).Key;
            }
            else
            {
                return Configuration.VehicleClassMapping.FirstOrDefault(x => x.Value == (VehicleClass)vehicle.ClassType).Key;
            }
        }

        public static string GetPlateFormatForVehicleClass(Vehicle vehicle)
        {
            var vehicleClassOptions = ConfigurationHelper.CheckVehicleConfiguration(vehicle);

            if (vehicleClassOptions != null)
            {
                var plateSets = vehicleClassOptions.plateSets;

                foreach (var plateSet in plateSets)
                {
                    int rd = new Random().Next(100);
                    if (rd < plateSet.plateProbability)
                    {
                        Console.WriteLine($"plateType: {plateSet.plateType}, plateFormat: {plateSet.plateFormat}, plateProbability: {plateSet.plateProbability}");
                        var transformedPlateFormat = UtilityHelper.TransformString(plateSet.plateFormat);
                        $"Plate format transformed for vehicle {vehicle.DisplayName}: {transformedPlateFormat}".ToLog(LogLevel.DEBUG);
                        return transformedPlateFormat;

                    }
                }
            }

            return null; 
        }
    }
}
