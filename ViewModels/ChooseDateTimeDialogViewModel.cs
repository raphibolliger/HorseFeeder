using System;

namespace HorseFeederAvalonia.ViewModels
{
    public class ChooseDateTimeDialogViewModel : ViewModelBase
    {
        public ChooseDateTimeDialogViewModel()
        {
            SelectedDate = DateTime.Now;
            SelectedHour = DateTime.Now.Hour;
            SelectedMinute = DateTime.Now.Minute;
        }

        public DateTime SelectedDate { get; set; }
        public int SelectedHour { get; set; }
        public int SelectedMinute { get; set; }
    }
}
