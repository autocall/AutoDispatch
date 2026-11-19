using System.Collections.Generic;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class AssignDatDriverViewModel : ViewModel {

        #region [ _fields ]

        private DatDriverViewModel _selectedDatDriver;
        private string _textDatDriverName;
        private string _textDatDriverPhone;
        private string _textDatDriverTruckNumber;
        private string _textDatDriverTrailerNumber;

        #endregion [ _fields ]

        #region [ Properties ]

        public LoadViewModel Load { get; }
        public List<DatDriverViewModel> DatDrivers { get; set; }
        
        public DatDriverViewModel SelectedDatDriver {
            get { return _selectedDatDriver; }
            set {
                if (_selectedDatDriver != value) {
                    _selectedDatDriver = value;
                    OnPropertyChanged();
                    if (value != null) {
                        TextDatDriverPhone = value.ContactPhone;
                        TextDatDriverTruckNumber = value.TruckNumber;
                        TextDatDriverTrailerNumber = value.TrailerNumber;
                        TextDatDriverName = value.Name;
                    } else {
                        TextDatDriverPhone = null;
                        TextDatDriverTruckNumber = null;
                        TextDatDriverTrailerNumber = null;
                        TextDatDriverName = null;
                    }
                }
            }
        }

        public string TextDatDriverName {
            get { return _textDatDriverName; }
            set {
                if (_textDatDriverName != value) {
                    _textDatDriverName = value;
                    if (SelectedDatDriver != null) {
                        SelectedDatDriver.Name = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public string TextDatDriverPhone {
            get { return _textDatDriverPhone; }
            set {
                if (_textDatDriverPhone != value) {
                    _textDatDriverPhone = value;
                    if (SelectedDatDriver != null) {
                        SelectedDatDriver.ContactPhone = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public string TextDatDriverTruckNumber {
            get { return _textDatDriverTruckNumber; }
            set {
                if (_textDatDriverTruckNumber != value) {
                    _textDatDriverTruckNumber = value;
                    if (SelectedDatDriver != null) {
                        SelectedDatDriver.TruckNumber = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public string TextDatDriverTrailerNumber {
            get { return _textDatDriverTrailerNumber; }
            set {
                if (_textDatDriverTrailerNumber != value) {
                    _textDatDriverTrailerNumber = value;
                    if (SelectedDatDriver != null) {
                        SelectedDatDriver.TrailerNumber = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Properties ]

        #region [ .ctor ]

        public AssignDatDriverViewModel(
            LoadViewModel load,
            IList<DatDriverViewModel> drivers) {
            DatDrivers = new List<DatDriverViewModel>();

            Load = load;
            this.DatDrivers.AddRange(drivers);
        }

        #endregion [ .ctor ]
    }
}
