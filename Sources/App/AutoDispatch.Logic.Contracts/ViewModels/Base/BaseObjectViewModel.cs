using System;
using System.Linq;
using AutoCall.Common.Interfaces;
using AutoDispatch.Logic.Attributes;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;

namespace AutoDispatch.Logic.ViewModels {
    public abstract class BaseObjectViewModel : BaseViewModel, ICloneable, IAssignable {

        public int EventId { get; set; }

        public abstract bool Filter(string search);
        public abstract bool Filter(FilterColumnsViewModel filterColumns);

        public virtual object Clone() {
            var clone = (BaseObjectViewModel)Activator.CreateInstance(GetType());
            foreach (var propertyInfo in GetType().GetProperties()) {
                if (propertyInfo.PropertyType.GetInterface(typeof(IAssignable).FullName) != null) {
                    var value = propertyInfo.GetValue(this, null);
                    if (value != null) {
                        if (propertyInfo.CanWrite &&
                            propertyInfo.GetCustomAttributes(typeof(NoCloneAttribute), false).Any() == false) {
                            propertyInfo.SetValue(clone, ((IAssignable)value).Clone(), null);
                        }
                    }
                } else {
                    if (propertyInfo.CanWrite && propertyInfo.CanRead) {
                        if (propertyInfo.GetCustomAttributes(typeof(NoCloneAttribute), false).Any() == false) {
                            propertyInfo.SetValue(clone, propertyInfo.GetValue(this, null), null);
                        }
                    }
                }
            }
            return clone;
        }

        public virtual void Assign(object source) {
            foreach (var propertyInfo in GetType().GetProperties()) {
                if (propertyInfo.PropertyType.GetInterface(typeof(IAssignable).FullName) != null) {
                    var value = propertyInfo.GetValue(source, null);
                    if (value != null) {
                        if (propertyInfo.CanWrite && propertyInfo.GetCustomAttributes(typeof(NoAssignAttribute), false).Any() == false) {
                            propertyInfo.SetValue(this, ((IAssignable) value).Clone(), null);
                        } else {

                        }
                    }
                } else {
                    if (propertyInfo.CanWrite && propertyInfo.CanRead) {
                        if (propertyInfo.GetCustomAttributes(typeof(NoAssignAttribute), false).Any() == false) {
                            propertyInfo.SetValue(this, propertyInfo.GetValue(source, null), null);
                        } else {

                        }
                    }
                }
            }
        }
    }
}