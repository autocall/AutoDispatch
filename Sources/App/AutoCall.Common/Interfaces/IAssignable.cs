using System;

namespace AutoCall.Common.Interfaces {
    public interface IAssignable : ICloneable {
        /// <summary>
        ///     Pulls all properties and sets to this object </summary>
        /// <param name="source">Source</param>
        void Assign(object source);
    }
}
