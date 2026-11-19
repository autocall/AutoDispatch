using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class HistoryLoadChangesViewModel : ViewModel {
        public LoadViewModel Origin { get; set; }
        public ObservableCollection<Wrapper1LoadViewModel> Wrappers { get; set; } = new ObservableCollection<Wrapper1LoadViewModel>();
        public object dc { get; set; }
    }
}
