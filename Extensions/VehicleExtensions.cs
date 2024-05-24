using GTA;
using LicensePlateChanger.Models;
using LicensePlateChanger.Utils;
using System;

namespace LicensePlateChanger.Extensions
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

        /// <summary>
        /// Updates vehicle's license plate information based on plate set.
        /// </summary>
        public static void UpdateVehicleLicensePlateInfo(Vehicle vehicle)
        {
            PlateSet newPlateSet = GetPlateSetForVehicleClass(vehicle);

            if (newPlateSet != null)
            {
                int currentPlateType = (int)vehicle.Mods.LicensePlateType;
                string currentPlateFormat = vehicle.Mods.LicensePlate;

                if (currentPlateType != newPlateSet.plateType)
                {
                    Helpers.UpdateLicensePlateType(vehicle, newPlateSet);
                }

                if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && currentPlateFormat != newPlateSet.plateFormat)
                {
                    if (!Helpers.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                    {
                        Helpers.UpdateLicensePlateFormat(vehicle, newPlateSet);
                    }
                }
            }
        }
        #endregion
    }
}
