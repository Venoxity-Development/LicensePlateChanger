using GTA;
using GTA.Native;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Tomlyn;
using Tomlyn.Model;

namespace LicensePlateChanger.Extensions
{
    internal static class VehicleExtensions
    {
        private static Dictionary<int, string> vehicleLicenseClassName = new Dictionary<int, string>();
        private static Dictionary<int, string> vehicleLicenseClassType = new Dictionary<int, string>();

        /// <summary>
        /// Checks if a vehicle is excluded based on its display name and the configuration settings.
        /// </summary>
        public static bool IsVehicleExcluded(Vehicle vehicle)
        {
            string className = Configuration.VehicleClassMapping.FirstOrDefault(x => x.Value == (VehicleClass)vehicle.ClassType).Key;

            if (Configuration.ConfigurationData.TryGetValue("vehicleClass", out var vehicleClassTable) && vehicleClassTable is TomlTable)
            {
                var vehicleClass = (TomlTable)vehicleClassTable;

                if (vehicleClass.TryGetValue(className, out var classEntry) && classEntry is TomlTable)
                {
                    var currentClassEntry = (TomlTable)classEntry;

                    if (currentClassEntry.TryGetValue("excludeVehicles", out var excludedVehicles) && excludedVehicles is TomlArray)
                    {
                        var excludedVehiclesList = ((TomlArray)excludedVehicles).ToArray();

                        foreach (var excludedVehicle in excludedVehiclesList)
                        {
                            if (excludedVehicle.ToString() == vehicle.DisplayName)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a vehicle is allowed for a specific vehicle class based on the configuration settings.
        /// </summary>
        public static bool IsVehicleAllowed(TomlTable currentClassEntry, string vehicleClass, Vehicle vehicle)
        {
            VehicleClass enumValue;

            if (Enum.TryParse(vehicleClass, out enumValue))
            {
                if ((VehicleClass)vehicle.ClassType == enumValue)
                {
                    if (currentClassEntry.TryGetValue("allowedVehicles", out var allowedVehicles) && allowedVehicles is TomlArray)
                    {
                        var allowedVehiclesList = ((TomlArray)allowedVehicles).ToArray();

                        foreach (var allowedVehicle in allowedVehiclesList)
                        {
                            int vehicleHash = Function.Call<int>(Hash.GET_HASH_KEY, allowedVehicle.ToString());

                            if (vehicleHash == vehicle.Model.Hash)
                            {
                                Console.WriteLine($"[LicensePlateChanger]: Vehicle {vehicle.DisplayName} is allowed for class {vehicleClass}");
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[LicensePlateChanger]: Failed to parse vehicle class. Invalid class: {vehicleClass}");
            }

            Console.WriteLine($"[LicensePlateChanger]: Vehicle {vehicle.DisplayName} is not allowed for class {vehicleClass}");
            return false;
        }

        /// <summary>
        /// Retrieves the class name for a given vehicle based on the configuration settings.
        /// </summary>
        private static string GetClassNameForVehicle(Vehicle vehicle)
        {
            List<VehicleClass> allowedClasses = new List<VehicleClass> { VehicleClass.Compacts, VehicleClass.Sedans };

            if (allowedClasses.Contains((VehicleClass)vehicle.ClassType))
            {
                return Configuration.VehicleClassMapping.FirstOrDefault(x => x.Value == VehicleClass.Cars).Key;
            } 
            else
            {
                return Configuration.VehicleClassMapping.FirstOrDefault(x => x.Value == (VehicleClass)vehicle.ClassType).Key;
            }
        }

        /// <summary>
        /// Retrieves the plate format for a specific vehicle class based on the configuration settings.
        /// </summary>
        private static string GetVehicleTypeClassName(Vehicle vehicle)
        {
            if (Configuration.ConfigurationData.TryGetValue("vehicleType", out var vehicleTypeTable) && vehicleTypeTable is TomlTable)
            {
                var vehicleTypeData = (TomlTable)vehicleTypeTable;

                foreach (var classEntry in vehicleTypeData.Values)
                {
                    if (classEntry is TomlTable currentClassEntry)
                    {
                        if (currentClassEntry.TryGetValue("className", out var vehicleClassObj))
                        {
                            var vehicleClass = vehicleClassObj.ToString().Replace("Vehicle", "");

                            if (currentClassEntry.TryGetValue("isTypeEnabled", out var isTypeEnabled))
                            {
                                if ((bool)isTypeEnabled)
                                {
                                    if (IsVehicleAllowed(currentClassEntry, vehicleClass, vehicle))
                                    {
                                        int vehicleID = vehicle.Handle;
                                        if (!vehicleLicenseClassType.ContainsKey(vehicleID))
                                        {
                                            if (currentClassEntry.TryGetValue("plateType", out var plateType))
                                            {
                                                int currentPlateType = (int)vehicle.Mods.LicensePlateType;
                                                string newPlateType = plateType.ToString();

                                                if (currentPlateType != int.Parse(newPlateType))
                                                {
                                                    Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, int.Parse(plateType.ToString()));

                                                    vehicleLicenseClassType[vehicleID] = newPlateType;
                                                }
                                            }
                                        }

                                        if (currentClassEntry.TryGetValue("plateFormat", out var plateFormat))
                                        {
                                            var transformedPlateFormat = Helpers.TransformString(plateFormat.ToString());
                                            Console.WriteLine($"[LicensePlateChanger]: Plate format transformed for vehicle class {vehicleClass}: {transformedPlateFormat}");
                                            return transformedPlateFormat;
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[LicensePlateChanger]: Vehicle class {vehicleClass} is disabled.");
                                    return null;
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the plate format for a vehicle class based on the configuration settings.
        /// </summary>
        public static string GetPlateFormatForVehicleClass(Vehicle vehicle)
        {
            string className = GetClassNameForVehicle(vehicle);

            if (Configuration.ConfigurationData.TryGetValue("vehicleClass", out var vehicleClassTable) && vehicleClassTable is TomlTable)
            {
                var vehicleClass = (TomlTable)vehicleClassTable;

                if (vehicleClass.TryGetValue(className, out var classEntry) && classEntry is TomlTable)
                {
                    var currentClassEntry = (TomlTable)classEntry;

                    if (currentClassEntry.TryGetValue("isEnabled", out var isEnabled))
                    {
                        if ((bool)isEnabled)
                        {
                            Console.WriteLine("currentClassEntry: " + Toml.FromModel(currentClassEntry));

                            int vehicleID = vehicle.Handle;
                            if (!vehicleLicenseClassName.ContainsKey(vehicleID))
                            {
                                if (currentClassEntry.TryGetValue("plateType", out var plateType))
                                {
                                    int currentPlateType = (int)vehicle.Mods.LicensePlateType;
                                    string newPlateType = plateType.ToString();

                                    if (currentPlateType != int.Parse(newPlateType))
                                    {
                                        Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, int.Parse(plateType.ToString()));

                                        vehicleLicenseClassName[vehicleID] = newPlateType;
                                    }
                                }
                            }

                            if (currentClassEntry.TryGetValue("plateFormat", out var plateFormat))
                            {
                                // # WIP if has vehicle type then cant use plate format inside vehicle class we want one to override all vehicles if not in list/type
                                if (string.IsNullOrEmpty(plateFormat.ToString()))
                                {
                                    return GetVehicleTypeClassName(vehicle);
                                }
                                else
                                {
                                    return Helpers.TransformString(plateFormat.ToString());
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }
    }
}
