using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LicensePlateChanger.Utils
{
    internal enum LogLevel
    {
        DEBUG, // Only prints if DEBUG flag is enabled
        INFO, // Prints to log file only
        DEVMODE, // Prints to console aswell
        ERROR, // Prints to console aswell
        FATAL // Prints to console aswell
    }

    internal static class Log
    {
        private const string Path = "Scripts\\LicensePlateChanger.log";

        static Log()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = $"{version.Major}.{version.Minor}";
            var message = $"LPC - License Plate Changer v{versionString}";
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;

            var writer = new StreamWriter(Path, false);
            writer.WriteLine(message);
            writer.Close();
        }

        public static void Write(string text, LogLevel logLevel = LogLevel.INFO)
        {
            if (Settings.EnableLogging == "false")
                return;

#if !DEBUG
             if (logLevel == LogLevel.DEBUG) return;
#endif
            if (logLevel > LogLevel.INFO) Console.WriteLine($"[{logLevel}] {text}");

            var writer = new StreamWriter(Path, true);
            writer.WriteLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] [{logLevel}] {text}");
            writer.Close();
        }

        internal static void Terminate()
        {
            if (!Directory.Exists($"Logs"))
                Directory.CreateDirectory($"Logs");
            if (!Directory.Exists($"Logs\\LicensePlateChanger"))
                Directory.CreateDirectory($"Logs\\LicensePlateChanger");

            File.Copy(Path, $"Logs\\LicensePlateChanger\\LPC_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.log");
        }
    }

    internal static class LogExtensions
    {
        public static void ToLog(this string message, LogLevel logLevel = LogLevel.INFO) =>
            Log.Write(message, logLevel);
    }
}
