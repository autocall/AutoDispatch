using System;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Contacts;

namespace AutoDispatch.Logic.ViewModels.Items {
    /// <summary>
    ///     Shipper or Receiver compnay View Model </summary>
    public class ShipperReceiverCompanyViewModel : BaseViewModel {

        public string Name{ get; set; }

        public ContactsViewModel Contacts { get; set; }

        public bool Filter(string search) {
            if (String.IsNullOrEmpty(search)) {
                return true;
            }
            search = search.Trim().ToLower();
            return (Name != null && Name.ToLower().Contains(search)) ||
                   (Contacts != null && Contacts.ToString().ToLower().Contains(search));
        }
    }
}
