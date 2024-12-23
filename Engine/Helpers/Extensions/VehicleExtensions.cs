using GTA;
using GTA.Native;
using LicensePlateChanger.Engine.Data;
using LicensePlateChanger.Engine.InternalSystems;
using System;

namespace LicensePlateChanger.Engine.Helpers.Extensions
{
    /// <summary>
    /// Provides extensions for managing vehicle-related functionalities.
    /// </summary>
    internal static class VehicleExtensions
    {
        #region Fields
        private static Random random = new Random();
        #endregion

        #region License Plate Management
        /// <summary>
        /// Retrieves plate set based on vehicle class configuration.
        /// </summary>
        public static PlateSet GetPlateSetForVehicleClass(Vehicle vehicle)
        {
            var vehicleClassOptions = ConfigurationHelper.CheckVehicleClassConfiguration(vehicle);
            if (vehicleClassOptions == null) return null;

            var vehicleTypeOptions = ConfigurationHelper.CheckVehicleTypeConfiguration(vehicle);
            var plateSets = vehicleTypeOptions?.plateSets ?? vehicleClassOptions.plateSets;

            foreach (var plateSet in plateSets)
            {
                if (random.Next(100) < plateSet.plateProbability)
                {
                    return plateSet;
                }
            }

            return null;
        }

        /// <summary>
        /// Updates vehicle's license plate information based on plate set.
        /// </summary>
        public static void UpdateVehicleLicensePlateInfo(Vehicle vehicle)
        {
            PlateSet newPlateSet = GetPlateSetForVehicleClass(vehicle);
            if (newPlateSet == null) return;

            int currentPlateType = (int)vehicle.Mods.LicensePlateType;
            string currentPlateFormat = vehicle.Mods.LicensePlate;

            if (currentPlateType != newPlateSet.plateType)
            {
                UtilityHelper.UpdateLicensePlateType(vehicle, newPlateSet);
            }

            if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && currentPlateFormat != newPlateSet.plateFormat)
            {
                if (!UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                {
                    UtilityHelper.UpdateLicensePlateFormat(vehicle, newPlateSet);
                }
            }
        }
        #endregion

        #region Vehicle Processing

        /// <summary>
        /// Processes a vehicle to check if it passes all required checks.
        /// </summary>
        /// <param name="vehicle">The vehicle to process</param>
        /// <returns>True if the vehicle passes all checks, otherwise false</returns>
        public static bool Process(this Vehicle vehicle)
        {
            int vehicleID = vehicle.Handle;

            if (!vehicle.Exists())
            {
                return false;
            }

            if (Globals.vehicleLicenseClassName.ContainsKey(vehicleID) && Globals.vehicleLicensePlates.ContainsKey(vehicleID))
            {
                return false;
            }

            if (Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle, "excludeVehicle"))
            {
                return false;
            }

            if (!ConfigurationManager.VehicleClassMapping.ContainsValue((VehicleClass)vehicle.ClassType))
            {
                return false;
            }

            if (ConfigurationHelper.IsVehicleExcluded(vehicle))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
