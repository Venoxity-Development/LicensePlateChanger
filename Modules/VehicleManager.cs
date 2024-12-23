using GTA;
using GTA.Native;
using GTA.UI;
using LicensePlateChanger.Engine.Helpers.Extensions;
using LicensePlateChanger.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LicensePlateChanger.Engine.InternalSystems;

namespace LicensePlateChanger.Modules
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
                        Engine.InternalSystems.Settings.LoadSettings();
                    }
                    catch (FileNotFoundException)
                    {
                        Engine.InternalSystems.Settings.LoadDefaultSettings();
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
            "OnTick event triggered.".ToLog();

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
            "Processing nearby vehicles...".ToLog();

            // Get all vehicles in the world
            Vehicle[] nearbyVehicles = World.GetAllVehicles();

            foreach (Vehicle vehicle in nearbyVehicles)
            {
                Wait(100); // Retain the delay for each vehicle

                // Skip vehicles that do not exist
                if (!vehicle.Exists())
                {
                    "Skipped: Vehicle does not exist.".ToLog();
                    continue;
                }

                int vehicleID = vehicle.Handle;

                // Skip if the vehicle is already in the license plate dictionary
                if (Globals.vehicleLicensePlates.ContainsKey(vehicleID))
                {
                    $"Skipped: Vehicle ID {vehicleID} is already in the license plate dictionary.".ToLog();
                    continue;
                }

                // Skip if the vehicle has the 'excludeVehicle' decorator
                if (Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle, "excludeVehicle"))
                {
                    $"Skipped: Vehicle ID {vehicleID} has 'excludeVehicle' decorator.".ToLog();
                    continue;
                }

                // Skip if the vehicle class type is not mapped
                if (!Configuration.VehicleClassMapping.ContainsValue((VehicleClass)vehicle.ClassType))
                {
                    $"Skipped: Vehicle ID {vehicleID} has an unmapped class type.".ToLog();
                    continue;
                }

                // Skip if the vehicle is excluded by configuration
                if (ConfigurationHelper.IsVehicleExcluded(vehicle))
                {
                    $"Skipped: Vehicle ID {vehicleID} is excluded by configuration.".ToLog();
                    continue;
                }

                // Update the vehicle's license plate information
                $"Updating license plate info for Vehicle ID {vehicleID}.".ToLog();
                VehicleExtensions.UpdateVehicleLicensePlateInfo(vehicle);
            }

            "Finished processing nearby vehicles.".ToLog();
        }
        #endregion
    }
}
