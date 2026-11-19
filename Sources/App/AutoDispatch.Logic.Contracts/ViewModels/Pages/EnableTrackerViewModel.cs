using System.Collections.Generic;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class EnableTrackerViewModel : ViewModel {

        #region [ _fields ]

        private string _textName;
        private string _textPhone;

        #endregion [ _fields ]

        #region [ Properties ]

        public string TextName {
            get { return _textName; }
            set {
                if (_textName != value) {
                    _textName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TextPhone {
            get { return _textPhone; }
            set {
                if (_textPhone != value) {
                    _textPhone = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion [ Properties ]
    }
}
