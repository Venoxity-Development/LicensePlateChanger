using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LicensePlateChanger.Engine.InternalSystems
{
    /// <summary>
    /// Defines different levels of logging.
    /// </summary>
    internal enum LogLevel
    {
        DEBUG, // Only prints if DEBUG flag is enabled
        INFO, // Prints to log file only
        DEVMODE, // Prints to console as well
        ERROR, // Prints to console as well
        FATAL // Prints to console as well
    }

    /// <summary>
    /// Handles logging operations for the License Plate Changer application.
    /// </summary>
    internal static class Log
    {
        #region Fields

        private const string Path = "Scripts\\LicensePlateChanger.log";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the log file with application version information.
        /// </summary>
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

        #endregion

        #region Methods

        /// <summary>
        /// Writes the provided text to the log file with the specified log level.
        /// </summary>
        /// <param name="text">The text to log.</param>
        /// <param name="logLevel">The log level.</param>
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

        /// <summary>
        /// Finalizes logging operations by creating log directories and copying the log file.
        /// </summary>
        internal static void Terminate()
        {
            if (!Directory.Exists($"Logs"))
                Directory.CreateDirectory($"Logs");
            if (!Directory.Exists($"Logs\\LicensePlateChanger"))
                Directory.CreateDirectory($"Logs\\LicensePlateChanger");

            File.Copy(Path, $"Logs\\LicensePlateChanger\\LPC_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.log");
        }

        #endregion
    }

    /// <summary>
    /// Provides extension methods for logging.
    /// </summary>
    internal static class LogExtensions
    {
        /// <summary>
        /// Writes the specified message to the log file with the given log level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="logLevel">The log level.</param>
        public static void ToLog(this string message, LogLevel logLevel = LogLevel.INFO) =>
            Log.Write(message, logLevel);
    }
}
