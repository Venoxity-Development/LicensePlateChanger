using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using LicensePlateChanger.Extensions;

namespace LicensePlateChanger
{
    public class Main : Script
    {
        #region Fields
        private Dictionary<int, string> vehicleLicensePlates = new Dictionary<int, string>();
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "neon_enabled", DecoratorType.Bool },
            { "neon_start", DecoratorType.Int },
            { "neon_known", DecoratorType.Bool },
            { "neon_effect", DecoratorType.Int },
            { "neon_base_r", DecoratorType.Int },
            { "neon_base_g", DecoratorType.Int },
            { "neon_base_b", DecoratorType.Int },
            { "neon_last_r", DecoratorType.Int },
            { "neon_last_g", DecoratorType.Int },
            { "neon_last_b", DecoratorType.Int }
        };
        #endregion

        public Main()
        {
            Decorators.Initialize();

            Tick += OnInit;
  
        }

        private void OnInit(object sender, EventArgs e)
        {
            Decorators.Register(decorators);

            Configuration.LoadConfiguration();

            Tick -= OnInit;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Wait(1000);
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character, 125f);
            foreach (Vehicle val in nearbyVehicles)
            {
                if (val.Exists())
                {
                    int vehicleID = val.Handle;
                    if (!vehicleLicensePlates.ContainsKey(vehicleID))
                    {
                        if (Configuration.VehicleClassMapping.ContainsValue((VehicleClass)val.ClassType))
                        {
                            if (!VehicleExtensions.IsVehicleExcluded(val))
                            {
                                string currentPlate = val.Mods.LicensePlate;
                                string newPlate = VehicleExtensions.GetPlateFormatForVehicleClass(val);

                                if (!string.IsNullOrEmpty(newPlate) && newPlate != currentPlate && !IsPlateAlreadyUsed(newPlate))
                                {
                                    Console.WriteLine($"[LicensePlateChanger]: Applying new license plate format {newPlate} to vehicle {val.DisplayName}.");

                                    val.Mods.LicensePlate = newPlate;

                                    vehicleLicensePlates[vehicleID] = newPlate;
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

        private bool IsPlateAlreadyUsed(string plate)
        {
            foreach (var kvp in vehicleLicensePlates)
            {
                if (kvp.Value == plate)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
