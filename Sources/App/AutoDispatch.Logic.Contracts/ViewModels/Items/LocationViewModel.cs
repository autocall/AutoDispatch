using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    /// <summary>
    ///     Location View Model </summary>
    public class LocationViewModel : ViewModel, ICloneable, IComparable {
        private string _formattedAddress;

        public float Longitude { get; set; }

        public float Latitude { get; set; }

        public string FormattedAddress {
            get { return _formattedAddress; }
            set {
                if (_formattedAddress != value) {
                    _formattedAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public LocationViewModel() {
            Latitude = (float)40.208978;
            Longitude = (float)-101.428187;
        }

        public object Clone() {
            return new LocationViewModel() { Latitude = Latitude, Longitude = Longitude, FormattedAddress = FormattedAddress };
        }

        public override string ToString() {
            return FormattedAddress;
        }

        public int CompareTo(object obj) {
            var loc = (LocationViewModel)obj;
            if (loc == null) return 1;
            if(loc.Latitude == this.Latitude && loc.Longitude == this.Longitude && loc.FormattedAddress == this.FormattedAddress) {
                return 0;
            }
            return 1;
        }
    }
}
