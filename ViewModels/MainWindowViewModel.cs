using HorseFeederAvalonia.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace HorseFeederAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ToggleService toggleService = new ToggleService();

        public ObservableCollection<SlotViewModel> Slots { get; set; }

        public MainWindowViewModel()
        {
            Slots = new ObservableCollection<SlotViewModel>(Enumerable.Range(1, 4).Select(i => new SlotViewModel(i, toggleService)));
        }
    }
}
