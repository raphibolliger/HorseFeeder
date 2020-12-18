using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace HorseFeederAvalonia.Services
{
    public class ToggleService
    {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

        public async Task ToggleSlot(P1 pin)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                await InternalToggle(pin);
                await Task.Delay(1000);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        private async Task<bool> InternalToggle(P1 pin)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                // set value to true
                Pi.Gpio[pin].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[pin].Value = true;
                Debug.WriteLine($"Set {pin} to ON");

                // reset the pin status to off
                var resetTimer = new System.Timers.Timer(2000);
                resetTimer.Elapsed += Callback;
                resetTimer.AutoReset = false;
                resetTimer.Start();
            }
            catch
            {
                Debug.WriteLine($"Could not toggle slot {pin} on Raspberry PI Hardware");
                tcs.SetResult(false);
            }

            void Callback(object sender, ElapsedEventArgs e)
            {
                try
                {
                    Pi.Gpio[pin].Value = false;
                    tcs.SetResult(true);
                    Debug.WriteLine($"Reset {pin} to OFF");
                }
                catch
                {
                    Debug.WriteLine($"Could not reset slot {pin} to off.");
                    tcs.SetResult(false);
                }
            }

            return await tcs.Task;
        }
    }
}
