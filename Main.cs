using System;
using System.Collections.Generic;
using GTA;
using LicensePlateChanger.Extensions;

namespace LicensePlateChanger
{
    public class Main : Script
    {
        private Dictionary<int, string> vehicleLicensePlates = new Dictionary<int, string>();

        public Main()
        {
            Configuration.LoadConfiguration();
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Wait(100);
            Vehicle[] allVehicles = World.GetAllVehicles();
            foreach (Vehicle val in allVehicles)
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

                                if (!string.IsNullOrEmpty(newPlate) && newPlate != currentPlate)
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
    }
}
