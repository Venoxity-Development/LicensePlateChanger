using GTA;
using GTA.Math;
using GTA.Native;
using LicensePlateChanger.Engine.Data;
using LicensePlateChanger.Engine.InternalSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tomlyn;
using Tomlyn.Syntax;

namespace LicensePlateChanger.Engine.Helpers
{
    internal static class UtilityHelper
    {
        #region Fields
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        #endregion

        #region Transformation
        /// <summary>
        /// Transforms a string by replacing '#' with a random digit, '?' with a random uppercase letter, and '.' with a random letter or number, with 50% probability of being either.
        /// </summary>
        /// <param name="input">The input string to transform</param>
        /// <returns>The transformed string</returns>
        public static string TransformString(string input)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in input)
            {
                if (c == '#')
                {
                    int randomNumber = Random.Next(10);
                    result.Append(randomNumber);
                }
                else if (c == '?')
                {
                    char randomChar = GetUniqueRandomChar();
                    result.Append(randomChar);
                }
                else if (c == '.')
                {
                    if (Random.Next(2) == 0) // 50% chance of being either a letter or a number
                    {
                        int randomNumber = Random.Next(10);
                        result.Append(randomNumber);
                    }
                    else
                    {
                        char randomChar = GetUniqueRandomChar();
                        result.Append(randomChar);
                    }
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        private static char GetUniqueRandomChar()
        {
            char randomChar;
            do
            {
                randomChar = (char)Random.Next('A', 'Z' + 1);
            } while (randomChar == 'I' || randomChar == 'O'); // Exclude 'I' and 'O' to avoid confusion with '1' and '0'
            return randomChar;
        }
        #endregion

        #region Plate Checking
        /// <summary>
        /// Checks if a given license plate is already in use.
        /// </summary>
        /// <param name="plate">The license plate to check</param>
        /// <returns>True if the plate is already in use, otherwise false</returns>
        public static bool IsPlateAlreadyUsed(string plate)
        {
            foreach (var kvp in Globals.vehicleLicensePlates)
            {
                if (kvp.Value == plate)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Plate Updates
        /// <summary>
        /// Updates the type of license plate for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate type will be updated</param>
        /// <param name="newPlateSet">The new plate set containing the updated plate type</param>
        public static void UpdateLicensePlateType(Vehicle vehicle, PlateSet newPlateSet)
        {
            Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
            Globals.vehicleLicenseClassName[vehicle.Handle] = newPlateSet.plateType;
        }

        /// <summary>
        /// Updates the format of the license plate for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate format will be updated</param>
        /// <param name="newPlateSet">The new plate set containing the updated plate format</param>
        public static void UpdateLicensePlateFormat(Vehicle vehicle, PlateSet newPlateSet)
        {
            string transformedPlateFormat = TransformString(newPlateSet.plateFormat);
            vehicle.Mods.LicensePlate = transformedPlateFormat;
            Globals.vehicleLicensePlates[vehicle.Handle] = transformedPlateFormat;
        }
        #endregion
    }

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
                if (ConfigurationManager.ConfigurationData.VehicleClassOptions.TryGetValue(targetClass, out var classOptions))
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
        /// Filters vehicles based on proximity to a given position.
        /// </summary>
        /// <param name="playerPosition">The player's position to filter by</param>
        /// <param name="radius">The radius to check for vehicles</param>
        /// <returns>Array of filtered vehicles</returns>
        public static Vehicle[] GetFilteredVehicles(Vector3 playerPosition, float radius)
        {
            return World.GetAllVehicles().Where(vehicle =>
                vehicle.Exists() &&
                vehicle.Position.DistanceTo(playerPosition) < radius).ToArray();
        }

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
                return ConfigurationManager.VehicleClassMapping.FirstOrDefault(x => x.Value == VehicleClass.Cars).Key;
            }
            else
            {
                return ConfigurationManager.VehicleClassMapping.FirstOrDefault(x => x.Value == (VehicleClass)vehicle.ClassType).Key;
            }
        }

        #endregion
    }
}
