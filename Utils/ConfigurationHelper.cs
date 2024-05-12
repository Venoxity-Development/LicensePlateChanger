using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace LicensePlateChanger
{
    public static class ConfigurationHelper
    {
        public static TomlTable LoadConfigurationFromFile(string filePath)
        {
            try
            {
                string tomlContent = File.ReadAllText(filePath);
                DocumentSyntax document = Toml.Parse(tomlContent);
                return document.ToModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading TOML file: {ex.Message}");
                return null;
            }
        }

        public static TomlTable GetTomlTable(string key)
        {
            if (Configuration.ConfigurationData.TryGetValue(key, out var value) && value is TomlTable table)
            {
                return table;
            }
            return null;
        }
    }
}
