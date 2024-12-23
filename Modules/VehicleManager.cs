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
using System.Linq;
using GTA.Math;

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

            ConfigurationManager.LoadConfiguration();

            switch (ConfigurationManager.ValidationState)
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

            // Fetch filtered vehicles (within 100m radius of the player)
            Vehicle[] nearbyVehicles = GetFilteredVehicles(Game.Player.Character.Position, 100f);
            int batchSize = 10; // Number of vehicles to process per batch
            int processedCount = 0;
            List<string> skippedReasons = new List<string>(); // For aggregated logging

            foreach (Vehicle vehicle in nearbyVehicles)
            {
                // Process in batches with a delay
                if (processedCount % batchSize == 0 && processedCount != 0)
                {
                    Wait(100); // Delay after processing each batch
                }

                processedCount++;
                if (!ProcessVehicle(vehicle, skippedReasons))
                {
                    continue;
                }

                // If valid, update license plate info
                VehicleExtensions.UpdateVehicleLicensePlateInfo(vehicle);
            }

            // Log skipped vehicles summary
            if (skippedReasons.Count > 0)
            {
                $"Skipped Vehicles: {string.Join(", ", skippedReasons)}".ToLog();
            }

            "Finished processing nearby vehicles.".ToLog();
        }

        // Filter vehicles based on proximity
        private static Vehicle[] GetFilteredVehicles(Vector3 playerPosition, float radius)
        {
            return World.GetAllVehicles().Where(vehicle =>
                vehicle.Exists() &&
                vehicle.Position.DistanceTo(playerPosition) < radius).ToArray();
        }

        // Process a single vehicle and return whether it passed all checks
        private static bool ProcessVehicle(Vehicle vehicle, List<string> skippedReasons)
        {
            int vehicleID = vehicle.Handle;

            // Skip if vehicle does not exist
            if (!vehicle.Exists())
            {
                skippedReasons.Add($"ID {vehicleID}: Does not exist");
                return false;
            }

            // Skip if already processed
            if (Globals.vehicleLicensePlates.ContainsKey(vehicleID))
            {
                skippedReasons.Add($"ID {vehicleID}: Already in dictionary");
                return false;
            }

            // Skip if it has the 'excludeVehicle' decorator
            if (Function.Call<bool>(Hash.DECOR_EXIST_ON, vehicle, "excludeVehicle"))
            {
                skippedReasons.Add($"ID {vehicleID}: Has 'excludeVehicle' decorator");
                return false;
            }

            // Skip if the vehicle class is unmapped
            if (!ConfigurationManager.VehicleClassMapping.ContainsValue((VehicleClass)vehicle.ClassType))
            {
                skippedReasons.Add($"ID {vehicleID}: Unmapped class type");
                return false;
            }

            // Skip if excluded by configuration
            if (ConfigurationHelper.IsVehicleExcluded(vehicle))
            {
                skippedReasons.Add($"ID {vehicleID}: Excluded by configuration");
                return false;
            }

            return true; // Passed all checks
        }
        #endregion
    }
}
