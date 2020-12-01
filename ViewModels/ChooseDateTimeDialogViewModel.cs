using System;

namespace HorseFeederAvalonia.ViewModels
{
    public class ChooseDateTimeDialogViewModel : ViewModelBase
    {
        public ChooseDateTimeDialogViewModel()
        {
            SelectedDate = DateTime.Now;
            SelectedHour = DateTime.Now.Hour;
            SelectedMinute = DateTime.Now.Minute + 1;
            Single = true;
            Daily = false;
            Weekly = false;
        }

        public DateTime SelectedDate { get; set; }
        public int SelectedHour { get; set; }
        public int SelectedMinute { get; set; }

        public bool Single { get; set; }
        public bool Daily { get; set; }
        public bool Weekly { get; set; }
    }
}
