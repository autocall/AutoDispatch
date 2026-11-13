using System;
using System.Linq;
using System.Reflection;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Base class of response from engine service </summary>
    [Serializable]
    public abstract class BaseEventResponseData {
        public PropertyInfo GetTargetProperty() {
            return GetType().GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(TargetResultAttribute), false).Any());
        }
        public object GetTarget() {
            return GetTargetProperty()?.GetValue(this);
        }
    }

    /// <summary>
    ///     Mark property for a convertation to a ViewModel </summary>
    public class TargetResultAttribute : Attribute { }
}
