using System;
using GTA;
using LicensePlateChanger.Modules;
using LicensePlateChanger.Engine.InternalSystems;

namespace LicensePlateChanger
{
    public class Main : Script
    {
        #region Fields
        private static VehicleManager VehicleManager = null;
        #endregion

        #region Constructors
        public Main()
        {
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

            Aborted += OnAborted;

            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnAborted(object sender, EventArgs e)
        {
            Log.Terminate();
        }
        #endregion
    }
}
