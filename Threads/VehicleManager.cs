using GTA;
using GTA.Native;
using GTA.UI;
using LicensePlateChanger.Extensions;
using LicensePlateChanger.Models;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LicensePlateChanger.Threads
{
    [ScriptAttributes(NoDefaultInstance = true)]
    public class VehicleManager : Script
    {
        #region Fields
        private bool processingStarted = false;
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "excludeVehicle", DecoratorType.Bool },
        };
        #endregion

        #region Constructors
        public VehicleManager()
        {
            Decorators.Initialize();

            Tick += OnInit;

            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnInit(object sender, EventArgs e)
        {
            #region Initialization
            Decorators.Register(decorators);

            Configuration.LoadConfiguration();

            switch (Configuration.ValidationState)
            {
                case VehicleClassMappingValidationState.Success:
                    try
                    {
                        Utils.Settings.LoadSettings();
                    }
                    catch (FileNotFoundException)
                    {
                        Utils.Settings.LoadDefaultSettings();
                    }
                    break;
                case VehicleClassMappingValidationState.FailureNoMapping:
                case VehicleClassMappingValidationState.FailureInvalidClassName:
                    Notification.Show("Failed: No mapping found for vehicle class or invalid class name.");
                    "Failed to initialize VehicleManager script.".ToLog();
                    Abort();
                    break;
                default:
                    Notification.Show("Unknown validation state encountered.");
                    "Failed to initialize VehicleManager script.".ToLog();
                    Abort();
                    break;
            }
            #endregion

            #region Event handling
            Tick -= OnInit;
            Tick += OnTick;
            #endregion
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!processingStarted)
            {
                "Vehicle processing started. Processing nearby vehicles...".ToLog();
                processingStarted = true; 
            }

            ProcessNearbyVehicles();
        }
        #endregion

        #region Methods
        private static void ProcessNearbyVehicles()
        {
            Vehicle[] nearbyVehicles = World.GetAllVehicles();
            foreach (Vehicle vehicle in nearbyVehicles)
            {
                Wait(100);
                if (vehicle.Exists())
                {
                    int vehicleID = vehicle.Handle;
                    if (!Globals.vehicleLicensePlates.ContainsKey(vehicleID))
                    {
                        if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle, "excludeVehicle"))
                        {
                            if (Configuration.ConfigurationData.VehicleClassOptions.ContainsKey((VehicleClass)vehicle.ClassType))
                            {
                                if (!VehicleExtensions.IsVehicleExcluded(vehicle))
                                {
                                    int currentPlateType = (int)vehicle.Mods.LicensePlateType;
                                    string currentPlateFormat = vehicle.Mods.LicensePlate;

                                    PlateSet newPlateSet = VehicleExtensions.GetPlateSetForVehicleClass(vehicle);

                                    if (newPlateSet != null)
                                    {
                                        string logMessage = "Vehicle's plate ";

                                        if (!string.IsNullOrEmpty(newPlateSet.plateType.ToString()) && currentPlateType != newPlateSet.plateType)
                                        {
                                            Function.Call<int>(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, vehicle, newPlateSet.plateType);
                                            Globals.vehicleLicenseClassName[vehicleID] = newPlateSet.plateType;
                                            logMessage += $"type changed to: {newPlateSet.plateType}";

                                            if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && newPlateSet.plateFormat != currentPlateFormat
                                                && !UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                                            {
                                                logMessage += ", ";
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(newPlateSet.plateFormat) && newPlateSet.plateFormat != currentPlateFormat
                                            && !UtilityHelper.IsPlateAlreadyUsed(newPlateSet.plateFormat))
                                        {
                                            var transformedPlateFormat = UtilityHelper.TransformString(newPlateSet.plateFormat);
                                            vehicle.Mods.LicensePlate = transformedPlateFormat;
                                            Globals.vehicleLicensePlates[vehicleID] = transformedPlateFormat;
                                            logMessage += $"format changed to: {transformedPlateFormat}";
                                        }

                                        if (logMessage != "Vehicle's plate ")
                                        {
                                            logMessage.ToLog();
                                        }
                                    }

                                }
                                else
                                {
                                    // Skip processing this vehicle if it's excluded
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
