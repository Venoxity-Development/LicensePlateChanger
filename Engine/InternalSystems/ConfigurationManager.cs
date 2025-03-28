using System.Reflection;
using System.Runtime.InteropServices;

namespace LicensePlateChanger.Engine.InternalSystems
{
    public static class ConfigurationManager
    {
        #region Configuration Properties and Methods
        public static VehicleData ConfigurationData { get; private set; }
        public static Dictionary<string, VehicleClass> VehicleClassMapping { get; private set; }
        public static VehicleClassMappingValidationState ValidationState { get; private set; }

        public static void LoadConfiguration()
        {
            Logger.Write("Attempting to load configuration data...", LogLevel.INFO);

            try
            {
                ConfigurationData = ConfigurationHelper.LoadConfigurationFromFile("./scripts/LicensePlateChanger/vehicleData.toml");

                if (ConfigurationData != null)
                {
                    Logger.Write("Configuration data successfully loaded.", LogLevel.INFO);
                    ValidationState = ValidateVehicleClassMapping();
                }
                else
                {
                    Logger.Write("Failed to load configuration data. The file may be missing or corrupted.", LogLevel.ERROR);
                }
            }
            catch (Exception ex)
            {
                Logger.Write($"Error while loading configuration: {ex.Message}", LogLevel.ERROR);
            }
        }

        private static VehicleClassMappingValidationState ValidateVehicleClassMapping()
        {
            Logger.Write("Validating vehicle class mapping...", LogLevel.INFO);

            if (ConfigurationData.VehicleClassOptions == null)
            {
                Logger.Write("No vehicle class mapping found in configuration.", LogLevel.ERROR);
                return VehicleClassMappingValidationState.FailureNoMapping;
            }

            VehicleClassMapping = new Dictionary<string, VehicleClass>();

            string[] classNames = { "Compacts", "Sedans", "SUVs", "Coupes", "Muscle", "SportsClassics", "Sports", "Super", "OffRoad", "Vans" };
            VehicleClass[] classValues = { VehicleClass.Compacts, VehicleClass.Sedans,
                                           VehicleClass.SUVs, VehicleClass.Coupes, VehicleClass.Muscle, VehicleClass.SportsClassics,
                                           VehicleClass.Sports, VehicleClass.Super, VehicleClass.OffRoad, VehicleClass.Vans };

            foreach (var classEntry in ConfigurationData.VehicleClassOptions)
            {
                var className = classEntry.Key.ToString();

                if (Enum.TryParse(className, true, out VehicleClass vehicleClassEnum))
                {
                    for (int i = 0; i < classNames.Length; i++)
                    {
                        VehicleClassMapping[classNames[i]] = classValues[i];
                    }

                    VehicleClassMapping[className] = vehicleClassEnum;
                }
                else
                {
                    Logger.Write($"Invalid vehicle class name: {className}.", LogLevel.ERROR);
                    return VehicleClassMappingValidationState.FailureInvalidClassName;
                }
            }

            Logger.Write("Vehicle class mapping validation completed successfully.", LogLevel.INFO);
            return VehicleClassMappingValidationState.Success;
        }
        #endregion
    }

    public class IniFile
    {
        #region Fields
        private readonly string _path;
        private readonly string _exe = Assembly.GetExecutingAssembly().GetName().Name;
        #endregion

        #region DLL Imports
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);
        #endregion

        #region Constructor
        public IniFile(string iniPath = null)
        {
            _path = new FileInfo(iniPath ?? _exe + ".ini").FullName;
        }
        #endregion

        #region Methods
        public string Read(string key, string section = null)
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(section ?? _exe, key, "", retVal, 255, _path);
            return retVal.ToString();
        }

        public void Write(string key, string value, string section = null)
        {
            WritePrivateProfileString(section ?? _exe, key, value, _path);
        }
        #endregion
    }

    public static class Settings
    {
        #region Properties
        public static string EnableLogging;
        public static string MaxNearbyVehicles;
        public static string VehicleScanRadius;
        #endregion

        #region Methods
        public static void LoadSettings()
        {
            var path = Path.Combine("scripts", "LicensePlateChanger.ini");
            var ini = new IniFile(path);

            EnableLogging = ini.Read("EnableLogging", "ADVANCED");
            MaxNearbyVehicles = ini.Read("MaxNearbyVehicles", "ADVANCED");
            VehicleScanRadius = ini.Read("VehicleScanRadius", "ADVANCED");

            float scanRadius = float.TryParse(VehicleScanRadius, out float result) ? result : 100.0f;
            int batchSize = int.TryParse(MaxNearbyVehicles, out int batchSizeResult) ? batchSizeResult : 10;

            Logger.Write($"Loaded EnableLogging setting: {EnableLogging}", LogLevel.DEBUG);
            Logger.Write($"Loaded MaxNearbyVehicles setting: {batchSize}", LogLevel.DEBUG);
            Logger.Write($"Loaded VehicleScanRadius setting: {scanRadius}", LogLevel.DEBUG);
        }

        public static void LoadDefaultSettings()
        {
            var path = Path.Combine("scripts", "LicensePlateChanger.ini");

            if (!File.Exists(path))
            {
                try
                {
                    File.Create(path).Close();
                    Logger.Write($"INI file created at {path}.", LogLevel.INFO);
                }
                catch (Exception ex)
                {
                    Logger.Write($"Error creating INI file at {path}: {ex.Message}", LogLevel.ERROR);
                }
            }
            else
            {
                Logger.Write($"INI file already exists at {path}. No action taken.", LogLevel.INFO);
            }
        }
        #endregion
    }
}
