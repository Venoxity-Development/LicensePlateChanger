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


        public static bool IsVehicleExcludedFromPlateChanging(Vehicle vehicle)
        {
            return false;
        }

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

    }
}
