using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using HorseFeederAvalonia.Enums;
using HorseFeederAvalonia.Services;
using HorseFeederAvalonia.Views.Dialoges;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.Timers;
using Unosquare.RaspberryIO.Abstractions;

namespace HorseFeederAvalonia.ViewModels
{
    public class SlotViewModel : ViewModelBase
    {
        private readonly ToggleService toggleService;
        private readonly DataPersistenceService dataPersistenceService;

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

        public SlotViewModel(int number, DateTime? expirationDate, RepetitionFrequency? repetitionFrequency, ToggleService toggleService, DataPersistenceService dataPersistenceService)
        {
            this.toggleService = toggleService;
            this.dataPersistenceService = dataPersistenceService;

            Number = number;
            BackGroundColor = Number % 2 == 0 ? Brushes.LightGray : Brushes.White;
            ToggleSlotCommand = ReactiveCommand.CreateFromTask(ToggleSlot);
            ChangeSlotTimeCommand = ReactiveCommand.CreateFromTask(ChooseSlot);

            // setup initial values for ExpirationDate and Repetition
            if (expirationDate.HasValue)
            {
                // repetition and date still good -> set values
                var now = DateTime.Now;
                if (repetitionFrequency.HasValue && expirationDate > now)
                {
                    RepetitionFrequency = repetitionFrequency;
                    ExpirationDate = expirationDate;
                    IsExpired = false;
                    Debug.WriteLine($"Slot {Number} set with repetition '{RepetitionFrequency}' next toggle at {ExpirationDate:F}");
                }
                // repetition and date already passed -> recalculate expiration date
                else if (repetitionFrequency.HasValue && expirationDate <= now)
                {
                    RepetitionFrequency = repetitionFrequency;
                    if (repetitionFrequency == Enums.RepetitionFrequency.Daily)
                    {
                        var nextDay = now.AddDays(1);
                        ExpirationDate = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, expirationDate.Value.Hour, expirationDate.Value.Minute, 0);
                    }
                    if (repetitionFrequency == Enums.RepetitionFrequency.Weekly)
                    {
                        var weekday = expirationDate.Value.DayOfWeek;
                        var nextDay = now.AddDays(1);
                        while(weekday != nextDay.DayOfWeek)
                        {
                            nextDay = nextDay.AddDays(1);
                        }
                        ExpirationDate = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, expirationDate.Value.Hour, expirationDate.Value.Minute, 0);
                    }
                    Debug.WriteLine($"Slot {Number} set with repetition '{RepetitionFrequency}' next toggle at {ExpirationDate:F}");
                }
                // repetition not set and date already passed -> clear init without values
                else if (expirationDate <= now)
                {
                    IsExpired = true;
                    Debug.WriteLine($"Slot {Number} date was expired, no repetition set. Last date was: {expirationDate:F}");
                }
                // repetition not send but date is good -> set only date
                else
                {
                    ExpirationDate = expirationDate;
                    IsExpired = false;
                    Debug.WriteLine($"Slot {Number} set without repetition, next toggle at {ExpirationDate:F}");
                }

                // setup timer if date is set
                if (ExpirationDate.HasValue)
                {
                    ToggleSlotAtGivenTime(ExpirationDate.Value);
                }
            }
            else
            {
                IsExpired = true;
            }
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

                    // save to configuration file
                    dataPersistenceService.SaveConfiguration(new Models.SlotConfiguration
                    {
                        Slot = Number,
                        ExpirationDate = result.SelectedDate,
                        RepetitionFrequency = result.RepetitionFrequency
                    });

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
