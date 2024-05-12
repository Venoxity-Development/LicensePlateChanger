using GTA;
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
            Console.WriteLine("[LicensePlateChanger]: Loading configuration data...");

            try
            {
                ConfigurationData = ConfigurationHelper.LoadConfigurationFromFile("./scripts/LicensePlateChanger/vehicleData.toml");

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
