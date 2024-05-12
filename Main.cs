using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using LicensePlateChanger.Threads;
using LicensePlateChanger.Utils;

namespace LicensePlateChanger
{
    public class Main : Script
    {
        #region Fields
        private static VehicleManager VehicleManager = null;
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "excludeVehicle", DecoratorType.Bool },
        };
        #endregion

        #region Constructors
        public Main()
        {
            Decorators.Initialize();

            if (VehicleManager == null)
            {
                "Instantiating VehicleManager script...".ToLog();
                VehicleManager = InstantiateScript<VehicleManager>();
            }
            else
            {
                "Aborting existing VehicleManager.".ToLog();
                VehicleManager.Abort();
                VehicleManager = null;
            }

            Tick += OnInit;
            Aborted += OnAborted;

            Interval = 1000;
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

            Tick -= OnInit;
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Log.Terminate();
        }
        #endregion
    }
}
