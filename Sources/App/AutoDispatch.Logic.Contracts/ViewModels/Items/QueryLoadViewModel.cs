using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class QueryLoadViewModel {
        public string City { get; set; }
        public string Address { get; set; }
        public LocationViewModel Location { get; set; }
        public string Contacts { get; set; }
        public string Company { get; set; }
        /// <summary>
        ///     Shipper or Receiver compnay </summary>
        public Guid? CompanyId { get; set; }
    }
}
