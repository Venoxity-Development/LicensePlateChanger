using GTA;
using GTA.Native;
using LicensePlateChanger.Models;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LicensePlateChanger.Extensions
{
    internal static class VehicleExtensions
    {
        private static Random random = new Random();

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

        public static PlateSet GetPlateSetForVehicleClass(Vehicle vehicle)
        {
            var vehicleClassOptions = ConfigurationHelper.CheckVehicleConfiguration(vehicle);

            if (vehicleClassOptions != null)
            {
                var plateSets = vehicleClassOptions.plateSets;

                foreach (var plateSet in plateSets)
                {
                    int rd = random.Next(100);
                    if (rd < plateSet.plateProbability)
                    {
                        return plateSet;
                    }
                }
            }

            return null;
        }

        public static void UpdateVehiclePlateInfo(Vehicle vehicle)
        {
            int currentPlateType = (int)vehicle.Mods.LicensePlateType;
            string currentPlateFormat = vehicle.Mods.LicensePlate;

            PlateSet newPlateSet = GetPlateSetForVehicleClass(vehicle);

            if (newPlateSet != null)
            {
                string logMessage = "Vehicle's plate ";

                if (!string.IsNullOrEmpty(newPlateSet.plateType.ToString()) && currentPlateType != newPlateSet.plateType)
                {
                    Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
                    Globals.vehicleLicenseClassName[vehicle.Handle] = newPlateSet.plateType;
                    logMessage += $"type changed to: {newPlateSet.plateType}";

                    if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && newPlateSet.plateFormat != currentPlateFormat
                        && !UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                    {
                        logMessage += ", ";
                    }
                }

                if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && newPlateSet.plateFormat != currentPlateFormat
                    && !UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                {
                    var transformedPlateFormat = UtilityHelper.TransformString(newPlateSet.plateFormat);
                    vehicle.Mods.LicensePlate = transformedPlateFormat;
                    Globals.vehicleLicensePlates[vehicle.Handle] = transformedPlateFormat;
                    logMessage += $"format changed to: {transformedPlateFormat}";
                }

                if (logMessage != "Vehicle's plate ")
                {
                    logMessage.ToLog();
                }
            }
        }
    }
}
