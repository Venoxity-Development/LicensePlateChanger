using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using GTA.Native;
using LicensePlateChanger.Utils;

namespace LicensePlateChanger
{
    public class Main : Script
    {
        #region Fields
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "excludeVehicle", DecoratorType.Bool },
        };
        #endregion

        #region Constructors
        public Main()
        {
            Decorators.Initialize();

            Tick += OnInit;
            Aborted += OnAborted;
        }
        #endregion

        #region Events
        private void OnInit(object sender, EventArgs e)
        {
            Decorators.Register(decorators);

            try
            {
                Utils.Settings.LoadSettings();
            }
            catch (FileNotFoundException)
            {
                Utils.Settings.LoadDefaultSettings();
            }

            Configuration.LoadConfiguration();

            Tick -= OnInit;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Wait(1000);
            Vehicle[] nearbyVehicles = World.GetAllVehicles();
            foreach (Vehicle val in nearbyVehicles)
            {
                if (val.Exists())
                {
                    int vehicleID = val.Handle;
                    if (!Globals.vehicleLicensePlates.ContainsKey(vehicleID))
                    {
                        if (!Function.Call<bool>(Hash.DECOR_EXIST_ON, val, "excludeVehicle"))
                        {
                            if (Configuration.VehicleClassMapping.ContainsValue((VehicleClass)val.ClassType))
                            {
                                //if (!VehicleExtensions.IsVehicleExcluded(val) && !Function.Call<bool>(Hash.DECOR_GET_BOOL, val, "excludeVehicle"))
                                //{
                                //    string currentPlate = val.Mods.LicensePlate;
                                //    string newPlate = VehicleExtensions.GetPlateFormatForVehicleClass(val);

                                //    if (!string.IsNullOrEmpty(newPlate) && newPlate != currentPlate && !UtilityHelper.IsPlateAlreadyUsed(newPlate))
                                //    {
                                //        Console.WriteLine($"[LicensePlateChanger]: Applying new license plate format {newPlate} to vehicle {val.DisplayName}.");

                                //        val.Mods.LicensePlate = newPlate;

                                //        Globals.vehicleLicensePlates[vehicleID] = newPlate;
                                //    }
                                //}
                                //else
                                //{
                                //    // Skip processing this vehicle if it's excluded
                                //    continue;
                                //}
                            }
                        }
                    }
                }
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Log.Terminate();
        }
        #endregion
    }
}
