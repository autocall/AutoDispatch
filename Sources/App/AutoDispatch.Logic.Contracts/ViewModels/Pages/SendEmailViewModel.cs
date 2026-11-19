using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {

    #region [ ViewModel Wrapper ]

    public class Wrapper<T> : INotifyPropertyChanged {
        private T _item;

        public T Item {
            get { return _item; }
            set {
                _item = value;
                OnPropertyChanged();
            }
        }

        public Wrapper(T item) {
            Item = item;
        }
        public Wrapper() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Wrapper2<T1, T2> : Wrapper<T1> {
        private T2 _item2;

        public T2 Item2 {
            get { return _item2; }
            set {
                _item2 = value;
                OnPropertyChanged();
            }
        }

        public Wrapper2(T1 item1, T2 item2) : base(item1) {
            Item2 = item2;
        }
    }

    #endregion [ ViewModel Wrapper ]

    public class SendEmailViewModel:BaseViewModel {
        private Wrapper2<string, bool> factoringCompanyEmail;

        public ObservableCollection<Wrapper2<string, bool>> Emails { get; set; }

        public List<string> EmailList {
            get { return Emails.Where(e => e.Item2 && e.Item != null).Select(e => e.Item.ToLower().Trim()).ToList(); }
            set { Emails = new ObservableCollection<Wrapper2<string, bool>>(value.Select(e => new Wrapper2<string, bool>(e, true))); }
        }

        public Wrapper2<string, bool> FactoringCompanyEmail {
            get => factoringCompanyEmail;
            set {
                factoringCompanyEmail = value;
                OnPropertyChanged();
            }
        }

        public bool SendInvoice { get; set; }
        public bool SendPOD { get; set; }
        public bool SendLumper { get; set; }
        public bool SendConfirmation { get; set; }
        public bool Merged { get; set; }

        public string Text { get; set; } = "Here's your invoice! We appreciate your prompt payment.\nThanks for your business!";
        public LoadViewModel Load { get; set; }

        public bool SaveContacts { get; set; } = true;
    }
}
