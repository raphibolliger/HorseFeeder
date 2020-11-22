using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using HorseFeederAvalonia.Views.Dialoges;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace HorseFeederAvalonia.ViewModels
{
    public class Slot : ViewModelBase
    {
        public int Number { get; }
        public IBrush BackGroundColor { get; }

        private DateTime? expirationDate;
        public DateTime? ExpirationDate
        {
            get => expirationDate;
            set
            {
                this.RaiseAndSetIfChanged(ref expirationDate, value);
                this.RaisePropertyChanged(nameof(ExpirationDateFormatted));
            }
        }

        public string ExpirationDateFormatted => expirationDate.HasValue ? expirationDate.Value.ToString("dddd, dd.MM.yyyy 'um' HH:mm 'Uhr'") : "---" ;

        private bool isExpired;
        public bool IsExpired
        {
            get => isExpired;
            set => this.RaiseAndSetIfChanged(ref isExpired, value);
        }

        public ReactiveCommand<Unit, Unit> ToggleSlotCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ChangeSlotTimeCommand { get; set; }

        public Slot(int number)
        {
            Number = number;
            BackGroundColor = Number % 2 == 0 ? Brushes.LightGray : Brushes.White;
            IsExpired = true;
            ToggleSlotCommand = ReactiveCommand.Create(ToggleSlot);
            ChangeSlotTimeCommand = ReactiveCommand.CreateFromTask(ChooseSlot);
        }

        public void ToggleSlot()
        {
            switch (Number)
            {
                case 1:
                    ToggleSlot(P1.Pin03);
                    break;
                case 2:
                    ToggleSlot(P1.Pin05);
                    break;
                case 3:
                    ToggleSlot(P1.Pin07);
                    break;
                case 4:
                    ToggleSlot(P1.Pin08);
                    break;
            }
        }

        private void ToggleSlot(P1 pin)
        {
            try
            {
                // set value to true
                Pi.Gpio[pin].PinMode = GpioPinDriveMode.Output;
                Pi.Gpio[pin].Value = true;

                // reset the pin status to off
                var resetTimer = new System.Timers.Timer(2000);
                resetTimer.Elapsed += Callback;
                resetTimer.AutoReset = false;
                resetTimer.Start();
            }
            catch
            {
                Debug.WriteLine($"Could not toggle slot {pin} on Raspberry PI Hardware");
            }

            void Callback(object sender, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    Pi.Gpio[pin].Value = false;
                    Debug.WriteLine($"Reseted {pin} value to off");
                }
                catch
                {
                    Debug.WriteLine($"Could not reset slot {pin} to off.");
                }
            }
        }

        private async Task ChooseSlot()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime deskotpLifeTime)
            {
                var selectedDateTime = await new ChooseDateTimeDialog().ShowDialog<DateTime>(deskotpLifeTime.MainWindow);
                if (selectedDateTime <= DateTime.Now) return;

                ExpirationDate = selectedDateTime;
                IsExpired = false;
                var secondsUntilToggle = selectedDateTime - DateTime.Now;
                var toggleTimer = new Timer(secondsUntilToggle.TotalMilliseconds);
                toggleTimer.Elapsed += ToggleTimerElapsed;
                toggleTimer.AutoReset = false;
                toggleTimer.Start();
            }
        }

        private void ToggleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ExpirationDate = null;
            IsExpired = true;
            ToggleSlot();
        }
    }
}
