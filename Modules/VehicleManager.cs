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

        #region Constructor
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
                    Abort();
                    break;
                default:
                    Notification.Show("Unknown validation state encountered.");
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
            if (!processingStarted)
            {
                // "Vehicle processing started. Processing nearby vehicles...".ToLog();
                processingStarted = true; 
            }

            ProcessNearbyVehicles();
        }
        #endregion

        #region Methods
        private static void ProcessNearbyVehicles()
        {
            Vehicle[] nearbyVehicles = ConfigurationHelper.GetFilteredVehicles(Game.Player.Character.Position, 100f);
            int batchSize = 10; 
            int processedCount = 0;
            foreach (Vehicle vehicle in nearbyVehicles)
            {
                if (processedCount % batchSize == 0 && processedCount != 0)
                {
                    Wait(100); 
                }

                processedCount++;
                if (!vehicle.Process())
                {
                    continue;
                }

                VehicleExtensions.UpdateVehicleLicensePlateInfo(vehicle);
            }
        }
        #endregion
    }
}
