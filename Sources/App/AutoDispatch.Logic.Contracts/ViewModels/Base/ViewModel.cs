using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AutoDispatch.Logic.ViewModels {
    /// <summary>
    ///     Represents base view model </summary>
    public class ViewModel : INotifyPropertyChanged/*, IDataErrorInfo*/ {
        /// <summary>
        ///     On property changed handler </summary>
        /// <param name="propertyName">Changed property name</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            // Raise event
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Occurs when property has been changed </summary>
        public event PropertyChangedEventHandler PropertyChanged;






        //string IDataErrorInfo.Error {
        //    get {
        //        return null;
        //        throw new NotSupportedException("IDataErrorInfo.Error is not supported, use IDataErrorInfo.this[propertyName] instead.");
        //    }
        //}
        //string IDataErrorInfo.this[string propertyName] {
        //    get {
        //        //if (string.IsNullOrEmpty(propertyName)) {
        //        //    throw new ArgumentException("Invalid property name", propertyName);
        //        //}
        //        //string error = string.Empty;
        //        //var value = GetValue(propertyName);
        //        //var results = new List<ValidationResult>(1);
        //        //var result = Validator.TryValidateProperty(
        //        //    value,
        //        //    new ValidationContext(this, null, null) {
        //        //        MemberName = propertyName
        //        //    },
        //        //    results);
        //        //if (!result) {
        //        //    var validationResult = results.First();
        //        //    error = validationResult.ErrorMessage;
        //        //}
        //        return "5555";
        //    }
        //}
        ////private object GetValue(string propertyName) {
        ////    PropertyInfo propInfo = GetType().GetProperty(propertyName);
        ////    return propInfo.GetValue(this);
        ////}
    }
}