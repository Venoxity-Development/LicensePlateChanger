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
        };
        #endregion

        public Main()
        {
            Decorators.Register(decorators);
            Configuration.LoadConfiguration();
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Wait(100);
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
