using HorseFeederAvalonia.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace HorseFeederAvalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ToggleService toggleService = new ToggleService();
        private readonly DataPersistenceService dataPersistenceService = new DataPersistenceService();

        public ObservableCollection<SlotViewModel> Slots { get; set; }

        public MainWindowViewModel()
        {
            var configs = dataPersistenceService.GetConfigurations();
            Slots = new ObservableCollection<SlotViewModel>(Enumerable.Range(1, 4).Select(i => {

                var slotConfiguration = configs.SingleOrDefault(s => s.Slot == i);
                if (slotConfiguration is null)
                {
                    return new SlotViewModel(i, null, null, toggleService, dataPersistenceService);
                }
                return new SlotViewModel(i, slotConfiguration.ExpirationDate, slotConfiguration.RepetitionFrequency, toggleService, dataPersistenceService);
            }));
        }
    }
}
