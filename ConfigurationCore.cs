using System;
using System.IO;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;

namespace LicensePlateChanger
{
    public static class ConfigurationCore
    {
        public static TomlTable Load(string filePath)
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
    }
}
