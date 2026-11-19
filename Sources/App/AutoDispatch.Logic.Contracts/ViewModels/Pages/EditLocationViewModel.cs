using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class EditLocationViewModel : ViewModel {
        /// <summary>
        ///     Locatiion </summary>
        public LoadLocationViewModel Location { get; set; }

        public string Comments { get; set; }

        public EditLocationViewModel(LoadLocationViewModel location) {
            Location = location;
        }
    }
}
