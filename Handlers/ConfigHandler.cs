using LicensePlateChanger.Models;
using System;
using System.Collections.Generic;

namespace LicensePlateChanger.Utils
{
    public class Configuration
    {
        #region Properties
        public static VehicleData ConfigurationData { get; private set; }
        public static Dictionary<string, VehicleClass> VehicleClassMapping { get; private set; }
        public static VehicleClassMappingValidationState ValidationState { get; private set; }
        #endregion

        #region Methods
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
                ValidationState = VehicleClassMappingValidationState.FailureNoMapping;
                return ValidationState;
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
                    // VehicleClass.Cars
                    for (int i = 0; i < classNames.Length; i++)
                    {
                        VehicleClassMapping[classNames[i]] = classValues[i];
                    }

                    // Regular Classes
                    VehicleClassMapping[className] = vehicleClassEnum;
                }
                else
                {
                    $"Invalid vehicle class name: {className}".ToLog(LogLevel.ERROR);
                    ValidationState = VehicleClassMappingValidationState.FailureInvalidClassName;
                    return ValidationState;
                }
            }

            "Vehicle class mapping validation completed successfully. All vehicle classes have been validated and mapped.".ToLog();
            ValidationState = VehicleClassMappingValidationState.Success;
            return ValidationState;
        }
        #endregion
    }
}
