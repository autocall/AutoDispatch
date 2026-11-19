using System.Collections.ObjectModel;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class YardsViewModel : ViewModel {
        public ObservableCollection<YardViewModel> Yards { get; set; } = new ObservableCollection<YardViewModel>();
    }
}
