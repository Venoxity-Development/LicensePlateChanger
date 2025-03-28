using Tomlyn;
using Tomlyn.Syntax;

namespace LicensePlateChanger.Engine.Helpers
{
    internal static class UtilityHelper
    {
        #region Fields
        internal static readonly Log Logger = new Log();
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        internal static Dictionary<int, string> vehicleLicensePlates = new Dictionary<int, string>();
        internal static Dictionary<int, int> vehicleLicenseClassName = new Dictionary<int, int>();
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
                result.Append(TransformCharacter(c));
            }

            return result.ToString();
        }

        private static char TransformCharacter(char c)
        {
            switch (c)
            {
                case '#':
                    return (char)('0' + Random.Next(10));  // Generate a random number directly
                case '?':
                    return GetUniqueRandomChar();  // Generate a random char (custom logic)
                case '.':
                    return Random.Next(2) == 0 ? (char)('0' + Random.Next(10)) : GetUniqueRandomChar();  // Random dot behavior (50% number or char)
                default:
                    return c;  // Return the character as is if no transformation is needed
            }
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
        /// Checks if the specified license plate is already in use by any vehicle.
        /// </summary>
        /// <param name="plate">The license plate to check</param>
        /// <returns>True if the plate is already in use by any vehicle, otherwise false</returns>
        public static bool IsPlateAlreadyUsed(string plate)
        {
            foreach (var kvp in vehicleLicensePlates)
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
            if (newPlateSet.plateType == -1) return;

            Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
            vehicleLicenseClassName[vehicle.Handle] = newPlateSet.plateType;
        }

        /// <summary>
        /// Updates the format of the license plate for a vehicle.
        /// </summary>
        /// <param name="vehicle">The vehicle whose license plate format will be updated</param>
        /// <param name="newPlateSet">The new plate set containing the updated plate format</param>
        public static void UpdateLicensePlateFormat(Vehicle vehicle, PlateSet newPlateSet)
        {
            if (newPlateSet.plateFormat == "XXXXXXXX") return;

            string transformedPlateFormat = TransformString(newPlateSet.plateFormat);
            vehicle.Mods.LicensePlate = transformedPlateFormat;
            vehicleLicensePlates[vehicle.Handle] = transformedPlateFormat;
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
                Logger.Write($"Error loading TOML file from {filePath}: {ex.Message}", LogLevel.ERROR);
                return null;
            }
        }

        #endregion

        #region Vehicle Configuration Checking

        /// <summary>
        /// Checks if a vehicle meets vehicle class configuration criteria.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns>The vehicle class options for the vehicle if it meets criteria; otherwise, null.</returns>
        public static VehicleClassOptions CheckVehicleClassConfiguration(Vehicle vehicle)
        {
            if (Enum.TryParse(GetClassNameForVehicle(vehicle), out VehicleClass targetClass) &&
                ConfigurationManager.ConfigurationData.VehicleClassOptions.TryGetValue(targetClass, out var classOptions) &&
                classOptions.isEnabled)
            {
                if (!vehicleLicensePlates.ContainsKey(vehicle.Handle))
                {
                    return classOptions;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a vehicle meets the vehicle type configuration criteria.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns>The vehicle type options if it meets criteria; otherwise, null.</returns>
        public static VehicleTypeOptions CheckVehicleTypeConfiguration(Vehicle vehicle)
        {
            string vehicleClassName = GetClassNameForVehicle(vehicle) + "Vehicle";

            if (ConfigurationManager.ConfigurationData?.VehicleTypeOptions == null)
            {
                return null;
            }

            foreach (var vehicleTypeOption in ConfigurationManager.ConfigurationData.VehicleTypeOptions.Values)
            {
                if (vehicleTypeOption != null && vehicleTypeOption.className == vehicleClassName && vehicleTypeOption.isTypeEnabled)
                {
                    if (vehicleTypeOption.allowedVehicles != null)
                    {
                        foreach (var allowedVehicle in vehicleTypeOption.allowedVehicles)
                        {
                            int allowedVehicleHash = Function.Call<int>(Hash.GET_HASH_KEY, allowedVehicle.ToString());

                            if (allowedVehicleHash == vehicle.Model.Hash && !vehicleLicensePlates.ContainsKey(vehicle.Handle))
                            {
                                return vehicleTypeOption;
                            }
                        }
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
            else if (vehicle.IsTrailer)
            {
                return ConfigurationManager.VehicleClassMapping.FirstOrDefault(x => x.Value == VehicleClass.Trailers).Key;
            }
            else
            {
                return ConfigurationManager.VehicleClassMapping.FirstOrDefault(x => x.Value == (VehicleClass)vehicle.ClassType).Key;
            }
        }
        
        #endregion
    }
}
