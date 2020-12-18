using Avalonia;
using Avalonia.ReactiveUI;
using System.Diagnostics;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace HorseFeederAvalonia
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            // init raspberry PI library
            try
            {
                Pi.Init<BootstrapWiringPi>();
                Pi.Gpio[Unosquare.RaspberryIO.Abstractions.P1.Pin07].Value = false;
                Pi.Gpio[Unosquare.RaspberryIO.Abstractions.P1.Pin11].Value = false;
                Pi.Gpio[Unosquare.RaspberryIO.Abstractions.P1.Pin13].Value = false;
                Pi.Gpio[Unosquare.RaspberryIO.Abstractions.P1.Pin15].Value = false;
            }
            catch
            {
                Debug.WriteLine("Could not initialize Raspberry PI Hardware");
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();
    }
}
