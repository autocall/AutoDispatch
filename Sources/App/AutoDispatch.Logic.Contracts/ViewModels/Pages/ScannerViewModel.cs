using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class ScannerViewModel : ViewModel {

        private BitmapSource _selectedImage;
        private IList<string> _scannerItemSource;
        private string _selectedScanner;
        private bool _aDF = true;
        private bool _bW = false;
        private bool _processing = false;
        private string _errorMessage;

        public BitmapSource SelectedImage {
            get => _selectedImage;
            set {
                _selectedImage = value;
                OnPropertyChanged();
            }
        }

        public string OkContent { get; set; } = "OK";

        public ObservableCollection<BitmapSource> Images { get; set; } = new ObservableCollection<BitmapSource>();
        public IList<string> ScannerItemSource {
            get => _scannerItemSource;
            set {
                _scannerItemSource = value;
                OnPropertyChanged();
            }
        }

        public string SelectedScanner {
            get => _selectedScanner;
            set {
                _selectedScanner = value;
                OnPropertyChanged();
            }
        }

        public bool ADF {
            get => _aDF;
            set {
                _aDF = value;
                OnPropertyChanged();
            }
        }
        public bool BW {
            get => _bW;
            set {
                _bW = value;
                OnPropertyChanged();
            }
        }
        public bool Processing {
            get => _processing;
            set {
                _processing = value;
                OnPropertyChanged();
            }
        }
        public string ErrorMessage {
            get => _errorMessage;
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public void Images_PropertyChanged() {
            base.OnPropertyChanged(nameof(Images));
        }
    }
}
