using GTA;
using LicensePlateChanger.Extensions;
using LicensePlateChanger.Models;
using System;
using System.IO;
using Tomlyn;
using Tomlyn.Syntax;

namespace LicensePlateChanger.Utils
{
    public static class ConfigurationHelper
    {
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

        public static VehicleClassOptions CheckVehicleConfiguration(Vehicle vehicle)
        {
            if (Enum.TryParse(VehicleExtensions.GetClassNameForVehicle(vehicle), out VehicleClass targetClass))
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
    }
}
