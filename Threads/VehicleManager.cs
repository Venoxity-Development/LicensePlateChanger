using GTA;
using LicensePlateChanger.Utils;
using System;
using System.Collections.Generic;
using Tomlyn;
using Tomlyn.Model;

namespace LicensePlateChanger.Threads
{
    [ScriptAttributes(NoDefaultInstance = true)]
    public class VehicleManager : Script
    {
        private static readonly Dictionary<string, DecoratorType> decorators = new Dictionary<string, DecoratorType>()
        {
            { "excludeVehicle", DecoratorType.Bool },
        };

        #region Constructors
        public VehicleManager()
        {
            Decorators.Initialize();

            Tick += OnInit;

            Interval = 1000;
        }
        #endregion

        #region Events
        private void OnInit(object sender, EventArgs e)
        {
            Decorators.Register(decorators);

            Configuration.LoadConfiguration();

            Tick -= OnInit;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            var tomlOut = Toml.FromModel(Configuration.ConfigurationData);
            Console.WriteLine(tomlOut);
        }
        #endregion
    }
}
