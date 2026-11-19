using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoCall.Logic.ViewModels {
    /// <summary>
    ///     Represents base view model </summary>
    public class BaseViewModel : INotifyPropertyChanged {
        /// <summary>
        ///     On property changed handler </summary>
        /// <param name="propertyName">Changed property name</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            // Raise event
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Occurs when property has been changed </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}