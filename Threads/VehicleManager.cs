using GTA;
using LicensePlateChanger.Utils;
using System;
using Tomlyn;
using Tomlyn.Model;

namespace LicensePlateChanger.Threads
{
    [ScriptAttributes(NoDefaultInstance = true)]
    public class VehicleManager : Script
    {
        #region Constructors
        public VehicleManager()
        {
            Tick += OnInit;

            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnInit(object sender, EventArgs e)
        {
            Configuration.LoadConfiguration();

            Tick -= OnInit;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Console.WriteLine("lol");

            var tomlOut = Toml.FromModel(Configuration.ConfigurationData);
            var model = Toml.ToModel<Models.VehicleClassTable>(tomlOut);
            $"{model.isEnabled}".ToLog();
        }
        #endregion
    }
}
