using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Linq;
using System.Windows;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class EditBookWrapperViewModel : ViewModel {
        public LoadViewModel Load { get; set; }
        public EditBookLoadViewModel Root { get; set; }

        public EditBookWrapperViewModel(LoadViewModel load, EditBookLoadViewModel root) {
            this.Load = load;
            this.Root = root;
        }

        public Visibility SendSmsVisibility {
            get {
                return String.IsNullOrEmpty(this.Root.ActualCompanies.FirstOrDefault(e => e.EntityId == Load.ActualCompanyId)?.SMS_Phone)
                    ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
