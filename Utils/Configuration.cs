using System;
using System.Collections.Generic;
using Tomlyn.Model;

namespace LicensePlateChanger.Utils
{
    public class Configuration
    {
        public static TomlTable ConfigurationData { get; private set; }
        public static Dictionary<string, VehicleClass> VehicleClassMapping { get; private set; }

        public static void LoadConfiguration()
        {
            "Loading configuration data...".ToLog();

            try
            {
                ConfigurationData = ConfigurationHelper.LoadConfigurationFromFile("./scripts/LicensePlateChanger/vehicleData.toml");

                if (ConfigurationData != null)
                {
                    "Configuration data loaded successfully.".ToLog();
                    ValidateVehicleClassMapping();
                }
                else
                {
                    "Failed to load configuration data.".ToLog(LogLevel.ERROR);
                }
            }
            catch (Exception ex)
            {
                $"Error loading configuration: {ex.Message}".ToLog(LogLevel.ERROR);
            }
        }

        private static void ValidateVehicleClassMapping()
        {
            "Validating vehicle class mapping...".ToLog();

            VehicleClassMapping = new Dictionary<string, VehicleClass>();

            string[] classNames = { "Compacts", "Sedans", "SUVs", "Coupes", "Muscle", "SportsClassics", "Sports", "Super", "OffRoad", "Vans" };
            VehicleClass[] classValues = { VehicleClass.Compacts, VehicleClass.Sedans, 
                VehicleClass.SUVs, VehicleClass.Coupes, VehicleClass.Muscle, VehicleClass.SportsClassics, 
                VehicleClass.Sports, VehicleClass.Super, VehicleClass.OffRoad, VehicleClass.Vans };

            if (ConfigurationData.TryGetValue("vehicleClass", out var vehicleClassTable) && vehicleClassTable is TomlTable)
            {
                var vehicleClass = (TomlTable)vehicleClassTable;

                foreach (var classEntry in vehicleClass)
                {
                    var className = classEntry.Key.ToString();
                    if (Enum.TryParse(className, true, out VehicleClass vehicleClassEnum))
                    {
                        // VehicleClass.Cars
                        for (int i = 0; i < classNames.Length; i++)
                        {
                            VehicleClassMapping[classNames[i]] = classValues[i];
                        }

                        // Regular Classes
                        VehicleClassMapping[className] = vehicleClassEnum;
                    }
                    else
                    {
                        $"Invalid vehicle class name: {className}".ToLog(LogLevel.ERROR);
                    }
                }

                "Vehicle class mapping validation completed successfully. All vehicle classes have been validated and mapped.".ToLog();
            }
            else
            {
                "No vehicle class mapping found.".ToLog(LogLevel.ERROR);
            }
        }
    }
}
