using System.Collections.Generic;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SearchCarrierLoadViewModel : ViewModel {
        private string _searchText;
       
        public LoadViewModel BrokerLoad { get; set; }
        public List<Wrapper1LoadViewModel> CarrierLoads { get; set; }

        public Wrapper1LoadViewModel SelectedItem { get; set; }

        public string SearchText {
            get { return _searchText; }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
