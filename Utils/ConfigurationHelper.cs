using GTA;
using LicensePlateChanger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tomlyn;
using Tomlyn.Syntax;

namespace LicensePlateChanger.Utils
{
    /// <summary>
    /// Helper class for loading and managing vehicle configurations.
    /// </summary>
    public static class ConfigurationHelper
    {
        #region Configuration Loading

        /// <summary>
        /// Loads vehicle configuration data from a TOML file.
        /// </summary>
        /// <param name="filePath">Path to the TOML file.</param>
        /// <returns>The loaded VehicleData object.</returns>
        public static VehicleData LoadConfigurationFromFile(string filePath)
        {
            try
            {
                string tomlContent = File.ReadAllText(filePath);
                DocumentSyntax document = Toml.Parse(tomlContent);

                var options = new TomlModelOptions();
                options.ConvertPropertyName = (string propertyName) => propertyName;

                return document.ToModel<VehicleData>(options);
            }
            catch (Exception ex)
            {
                $"Error loading TOML file: {ex.Message}".ToLog(LogLevel.ERROR);
                return null;
            }
        }

        #endregion

        #region Vehicle Configuration Checking

        /// <summary>
        /// Checks if a vehicle meets certain configuration criteria.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns>The class options for the vehicle if it meets criteria; otherwise, null.</returns>
        public static VehicleClassOptions CheckVehicleConfiguration(Vehicle vehicle)
        {
            if (Enum.TryParse(GetClassNameForVehicle(vehicle), out VehicleClass targetClass))
            {
                if (Configuration.ConfigurationData.VehicleClassOptions.TryGetValue(targetClass, out var classOptions))
                {
                    if (classOptions.isEnabled)
                    {
                        int vehicleID = vehicle.Handle;
                        if (!Globals.vehicleLicensePlates.ContainsKey(vehicleID))
                        {
                            return classOptions;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Vehicle Exclusion

        /// <summary>
        /// Checks if a vehicle is excluded from certain operations.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns>True if the vehicle is excluded; otherwise, false.</returns>
        public static bool IsVehicleExcluded(Vehicle vehicle)
        {
            return false; // Placeholder implementation
        }

        #endregion

        #region Vehicle Class Handling

        /// <summary>
        /// Retrieves the class name of a vehicle based on its type.
        /// </summary>
        /// <param name="vehicle">The vehicle to get the class name for.</param>
        /// <returns>The class name of the vehicle.</returns>
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
    }
}
