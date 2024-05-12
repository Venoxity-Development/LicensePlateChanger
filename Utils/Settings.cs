using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace LicensePlateChanger.Utils
{
    class IniFile
    {
        #region Fields
        readonly string _path;
        readonly string _exe = Assembly.GetExecutingAssembly().GetName().Name;
        #endregion

        #region DLL Imports
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);
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

    internal static class Settings
    {
        #region Properties
        public static string EnableLogging;
        #endregion

        #region Methods
        internal static void LoadSettings()
        {
            var path = Path.Combine("scripts", "LicensePlateChanger.ini");
            var ini = new IniFile(path);

            EnableLogging = ini.Read("EnableLogging", "ADVANCED");

            $"Loaded EnableLogging setting: {EnableLogging}".ToLog();
        }

        internal static void LoadDefaultSettings()
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
                    (ex + "Error creating INI file").ToLog(LogLevel.ERROR);
                    return;
                }
            }
        }
        #endregion
    }
}
