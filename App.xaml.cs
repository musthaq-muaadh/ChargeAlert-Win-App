using System.Threading;
using System.Windows;

namespace ChargeAlert
{
    public partial class App : Application
    {
        Mutex? _mutex;
        BatteryMonitor? _monitor;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // only one copy at a time
            _mutex = new Mutex(true, "ChargeAlert.SingleInstance", out bool created);
            if (!created) { Shutdown(); return; }

            _monitor = new BatteryMonitor();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _monitor?.Dispose();
            base.OnExit(e);
        }
    }
}
