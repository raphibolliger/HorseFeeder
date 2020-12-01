using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using HorseFeederAvalonia.Enums;
using HorseFeederAvalonia.Services;
using HorseFeederAvalonia.Views.Dialoges;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using Unosquare.RaspberryIO.Abstractions;

namespace HorseFeederAvalonia.ViewModels
{
    public class SlotViewModel : ViewModelBase
    {
        private readonly ToggleService toggleService;

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

        public string ExpirationDateFormatted
        { 
            get
            {
                if (expirationDate.HasValue)
                {
                    switch (repetitionFrequency)
                    {
                        case Enums.RepetitionFrequency.Daily: return "Täglich um " + expirationDate.Value.ToString("HH:mm 'Uhr'");
                        case Enums.RepetitionFrequency.Weekly: return "Wöchentlich am " + expirationDate.Value.ToString("dddd 'um' HH:mm 'Uhr'");
                        default: return "Einmalig am " + expirationDate.Value.ToString("dddd, dd.MM.yyyy 'um' HH:mm:ss 'Uhr'");
                    }
                }

                return "---";
            }
        }

        private bool isExpired;
        public bool IsExpired
        {
            get => isExpired;
            set => this.RaiseAndSetIfChanged(ref isExpired, value);
        }

        private RepetitionFrequency? repetitionFrequency;
        public RepetitionFrequency? RepetitionFrequency
        {
            get => repetitionFrequency;
            set => this.RaiseAndSetIfChanged(ref repetitionFrequency, value);
        }

        public ReactiveCommand<Unit, Unit> ToggleSlotCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ChangeSlotTimeCommand { get; set; }

        public SlotViewModel(int number, ToggleService toggleService)
        {
            this.toggleService = toggleService;

            Number = number;
            BackGroundColor = Number % 2 == 0 ? Brushes.LightGray : Brushes.White;
            IsExpired = true;
            ToggleSlotCommand = ReactiveCommand.CreateFromTask(ToggleSlot);
            ChangeSlotTimeCommand = ReactiveCommand.CreateFromTask(ChooseSlot);
        }

        private async Task ToggleSlot()
        {
            switch (Number)
            {
                case 1:
                    await ToggleSlot(P1.Pin03);
                    break;
                case 2:
                    await ToggleSlot(P1.Pin05);
                    break;
                case 3:
                    await ToggleSlot(P1.Pin07);
                    break;
                case 4:
                    await ToggleSlot(P1.Pin11);
                    break;
            }
        }

        private async Task ToggleSlot(P1 pin)
        {
            await toggleService.ToggleSlot(pin);
        }

        private async Task ChooseSlot()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime deskotpLifeTime)
            {
                var result = await new ChooseDateTimeDialog().ShowDialog<DateTimeDialogResult>(deskotpLifeTime.MainWindow);
                if (result is not null)
                {
                    if (result.SelectedDate <= DateTime.Now) return;
                    RepetitionFrequency = result.RepetitionFrequency;
                    ToggleSlotAtGivenTime(result.SelectedDate.Value);
                }
            }
        }

        private void ToggleSlotAtGivenTime(DateTime givenTime)
        {
            ExpirationDate = givenTime;
            IsExpired = false;
            var secondsUntilToggle = givenTime - DateTime.Now;
            var toggleTimer = new Timer(secondsUntilToggle.TotalMilliseconds);
            toggleTimer.Elapsed += ToggleTimerElapsed;
            toggleTimer.AutoReset = false;
            toggleTimer.Start();
        }

        private async void ToggleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            switch (RepetitionFrequency)
            {
                case Enums.RepetitionFrequency.Daily:
                    ToggleSlotAtGivenTime(ExpirationDate.Value.AddDays(1));
                    break;
                case Enums.RepetitionFrequency.Weekly:
                    ToggleSlotAtGivenTime(ExpirationDate.Value.AddDays(7));
                    break;
                default:
                    ExpirationDate = null;
                    IsExpired = true;
                    break;
            }
            await ToggleSlot();
        }
    }
}
