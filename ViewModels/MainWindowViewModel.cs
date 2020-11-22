using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace HorseFeederAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Slot> Slots { get; set; }

        public MainWindowViewModel()
        {
            Slots = new ObservableCollection<Slot>(Enumerable.Range(1, 8).Select(i => new Slot(i)));
        }

    }
}
