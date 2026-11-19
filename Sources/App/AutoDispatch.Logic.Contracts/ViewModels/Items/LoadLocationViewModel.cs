using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class LoadLocationViewModel : ICloneable {
        public string Address { get; set; }
        public string City { get; set; }
        public LocationViewModel Location { get; set; }

        public object Clone() {
            return new LoadLocationViewModel() {
                Location = (LocationViewModel) Location.Clone(),
                Address = Address,
                City = City
            };
        }
    }
}
