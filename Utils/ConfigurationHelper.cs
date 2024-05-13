using LicensePlateChanger.Models;
using System;
using System.IO;
using Tomlyn;
using Tomlyn.Syntax;

namespace LicensePlateChanger.Utils
{
    public static class ConfigurationHelper
    {
        public static VehicleData LoadConfigurationFromFile(string filePath)
        {
            try
            {
                string tomlContent = File.ReadAllText(filePath);
                DocumentSyntax document = Toml.Parse(tomlContent);

                var options = new TomlModelOptions();
                options.ConvertPropertyName = (string propertyName) => propertyName;
                
                return document.ToModel<VehicleData>(options);
            }
            catch (Exception ex)
            {
                $"Error loading TOML file: {ex.Message}".ToLog(LogLevel.ERROR);
                return null;
            }
        }
    }
}
