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
            try
            {
                Decorators.Initialize();
                Logger.Write("Decorator system initialized.", LogLevel.DEBUG);

                foreach (var decorator in decorators)
                {
                    Logger.Write($"Registered decorator '{decorator.Key}' of type '{decorator.Value}'.", LogLevel.DEBUG);
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"An error occurred while initializing decorators: {ex.Message}", LogLevel.ERROR);
            }

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
                Logger.Write("Vehicle processing initiated. Scanning for nearby vehicles...", LogLevel.INFO);
                processingStarted = true; 
            }

            ProcessNearbyVehicles();
        }
        #endregion

        #region Methods
        private static void ProcessNearbyVehicles()
        {
            float scanRadius = float.TryParse(Engine.InternalSystems.Settings.VehicleScanRadius, out float result) ? result : 100.0f;
            int batchSize = int.TryParse(Engine.InternalSystems.Settings.MaxNearbyVehicles, out int batchSizeResult) ? batchSizeResult : 10;

            Vehicle[] nearbyVehicles = ConfigurationHelper.GetFilteredVehicles(Game.Player.Character.Position, scanRadius);
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
