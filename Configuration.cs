using GTA;
using System;
using System.Collections.Generic;
using Tomlyn.Model;

namespace LicensePlateChanger
{
    public class Configuration
    {
        public static TomlTable ConfigurationData { get; private set; }
        public static Dictionary<string, VehicleClass> VehicleClassMapping { get; private set; }

        public static void LoadConfiguration()
        {
            Console.WriteLine("[LicensePlateChanger]: Loading configuration data...");

            try
            {
                ConfigurationData = ConfigurationCore.Load("./scripts/LicensePlateChanger/vehicleData.toml");

                if (ConfigurationData != null)
                {
                    Console.WriteLine("[LicensePlateChanger]: Configuration data loaded successfully.");
                    ValidateVehicleClassMapping();
                }
                else
                {
                    Console.WriteLine("[LicensePlateChanger]: Failed to load configuration data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LicensePlateChanger]: Error loading configuration: {ex.Message}");
            }
        }

        private static void ValidateVehicleClassMapping()
        {
            Console.WriteLine("[LicensePlateChanger]: Validating vehicle class mapping...");

            VehicleClassMapping = new Dictionary<string, VehicleClass>();

            if (ConfigurationData.TryGetValue("vehicleClass", out var vehicleClassTable) && vehicleClassTable is TomlTable)
            {
                var vehicleClass = (TomlTable)vehicleClassTable;

                foreach (var classEntry in vehicleClass)
                {
                    var className = classEntry.Key.ToString();
                    if (Enum.TryParse(className, true, out VehicleClass vehicleClassEnum))
                    {
                        VehicleClassMapping[className] = vehicleClassEnum;
                    }
                    else
                    {
                        Console.WriteLine($"[LicensePlateChanger]: Invalid vehicle class name: {className}");
                    }
                }
            }
            else
            {
                Console.WriteLine("[LicensePlateChanger]: No vehicle class mapping found.");
            }
        }
    }
}
