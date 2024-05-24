using GTA;
using GTA.Native;
using LicensePlateChanger.Models;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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

        #region Vehicle Information Retrieval
        /// <summary>
        /// Determines if a vehicle is excluded from license plate changing.
        /// </summary>
        public static bool IsVehicleExcludedFromPlateChanging(Vehicle vehicle)
        {
            return false;
        }

        /// <summary>
        /// Retrieves the class name for a given vehicle.
        /// </summary>
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
                    UpdateLicensePlateType(vehicle, newPlateSet);
                }

                if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && currentPlateFormat != newPlateSet.plateFormat)
                {
                    if (!UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                    {
                        UpdateLicensePlateFormat(vehicle, newPlateSet);
                    }
                }
            }
        }

        /// <summary>
        /// Updates vehicle's license plate type.
        /// </summary>
        private static void UpdateLicensePlateType(Vehicle vehicle, PlateSet newPlateSet)
        {
            Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
            Globals.vehicleLicenseClassName[vehicle.Handle] = newPlateSet.plateType;
        }

        /// <summary>
        /// Updates vehicle's license plate format.
        /// </summary>
        private static void UpdateLicensePlateFormat(Vehicle vehicle, PlateSet newPlateSet)
        {
            string transformedPlateFormat = UtilityHelper.TransformString(newPlateSet.plateFormat);
            vehicle.Mods.LicensePlate = transformedPlateFormat;
            Globals.vehicleLicensePlates[vehicle.Handle] = transformedPlateFormat;
        }
        #endregion
    }
}
