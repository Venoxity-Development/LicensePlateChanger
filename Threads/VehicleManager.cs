using GTA;
using GTA.Native;
using GTA.UI;
using LicensePlateChanger.Extensions;
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
                                    VehicleExtensions.UpdateVehiclePlateInfo(vehicle);
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
