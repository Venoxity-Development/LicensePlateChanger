using GTA;
using LicensePlateChanger.Utils;
using System;

namespace LicensePlateChanger.Threads
{
    [ScriptAttributes(NoDefaultInstance = true)]
    public class VehicleManager : Script
    {
        #region Constructors
        public VehicleManager()
        {
            "VehicleManager initialized.".ToLog();

            Tick += OnTick;

            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnTick(object sender, EventArgs e)
        {
            ProcessNearbyVehicles();
        }
        #endregion

        #region Methods
        public static void ProcessNearbyVehicles()
        {

        }
        #endregion
    }
}
