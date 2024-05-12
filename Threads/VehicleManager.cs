using GTA;
using System;

namespace LicensePlateChanger.Threads
{
    [ScriptAttributes(NoDefaultInstance = true)]
    public class VehicleManager : Script
    {
        public VehicleManager()
        {
            Tick += OnTick;

            Interval = 1000;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Console.WriteLine("Tick 2");
        }
    }
}
