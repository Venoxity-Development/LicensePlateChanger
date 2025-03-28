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
            "Loading configuration data...".ToLog();

            try
            {
                ConfigurationData = ConfigurationHelper.LoadConfigurationFromFile("./scripts/LicensePlateChanger/vehicleData.toml");

                if (ConfigurationData != null)
                {
                    "Configuration data loaded successfully.".ToLog();
                    ValidationState = ValidateVehicleClassMapping();
                }
                else
                {
                    "Failed to load configuration data.".ToLog(LogLevel.ERROR);
                }
            }
            catch (Exception ex)
            {
                $"Error loading configuration: {ex}".ToLog(LogLevel.ERROR);
            }
        }

        private static VehicleClassMappingValidationState ValidateVehicleClassMapping()
        {
            "Validating vehicle class mapping...".ToLog();

            if (ConfigurationData.VehicleClassOptions == null)
            {
                "No vehicle class mapping found.".ToLog(LogLevel.ERROR);
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
                    $"Invalid vehicle class name: {className}".ToLog(LogLevel.ERROR);
                    return VehicleClassMappingValidationState.FailureInvalidClassName;
                }
            }

            "Vehicle class mapping validation completed successfully.".ToLog();
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
        #endregion

        #region Methods
        public static void LoadSettings()
        {
            var path = Path.Combine("scripts", "LicensePlateChanger.ini");
            var ini = new IniFile(path);

            EnableLogging = ini.Read("EnableLogging", "ADVANCED");
            $"Loaded EnableLogging setting: {EnableLogging}".ToLog(LogLevel.DEBUG);
        }

        public static void LoadDefaultSettings()
        {
            var path = Path.Combine("scripts", "LicensePlateChanger.ini");

            if (!File.Exists(path))
            {
                try
                {
                    File.Create(path).Close();
                    $"INI file created: {path}".ToLog();
                }
                catch (Exception ex)
                {
                    $"Error creating INI file: {ex}".ToLog(LogLevel.ERROR);
                }
            }
        }
        #endregion
    }
}
