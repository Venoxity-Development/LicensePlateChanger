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
                Logger.Write("No existing VehicleManager found. Initializing a new instance...", LogLevel.INFO);
            }
            else
            {
                Logger.Write("Existing VehicleManager detected. Aborting and reinitializing...", LogLevel.ERROR);
                _vehicleManager.Abort();
                Logger.Write("VehicleManager successfully aborted.", LogLevel.INFO);
            }

            _vehicleManager = InstantiateScript<VehicleManager>();

            if (_vehicleManager != null)
                Logger.Write("VehicleManager successfully initialized.", LogLevel.INFO);
            else
                Logger.Write("Failed to initialize VehicleManager.", LogLevel.ERROR);

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
