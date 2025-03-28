namespace LicensePlateChanger
{
    public class LicensePlateChanger : Script
    {
        #region Fields
        private static VehicleManager _vehicleManager;
        #endregion

        #region Constructor
        public LicensePlateChanger()
        {
            if (_vehicleManager == null)
            {
                Logger.Write("Initializing VehicleManager script...", LogLevel.INFO);
                _vehicleManager = InstantiateScript<VehicleManager>();
            }
            else
            {
                Logger.Write("Existing VehicleManager detected. Aborting...", LogLevel.ERROR);
                _vehicleManager.Abort();
                _vehicleManager = null;

                Logger.Write("VehicleManager successfully aborted.", LogLevel.INFO);
            }

            Aborted += OnAborted;
            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnAborted(object sender, EventArgs e)
        {
            Logger.Write("LicensePlateChanger script aborted unexpectedly.", LogLevel.FATAL);
            Logger.Terminate();
        }
        #endregion
    }
}
