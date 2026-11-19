using System;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class ScanSchemeSendViewModel : ViewModel {

        private byte[] pdfData;
        private string fileTempPath;
        private string fileName;

        public string Trailer { get; set; }

        public Guid ActualCompanyId { get; set; }

        public string Emails { get; set; }

        public byte[] PdfData {
            get => pdfData;
            set {
                pdfData = value;
                OnPropertyChanged();
            }
        }

        public string FileName {
            get => fileName;
            set {
                fileName = value;
                OnPropertyChanged();
            }
        }

        public string FileTempPath {
            get => fileTempPath;
            set {
                fileTempPath = value;
                OnPropertyChanged();
            }
        }
    }
}
