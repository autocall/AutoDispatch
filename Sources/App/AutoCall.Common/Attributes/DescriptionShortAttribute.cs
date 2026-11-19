using System;

namespace AutoCall.Common.Attributes {
    //
    // Summary:
    //     Specifies a description for a property or event.
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionShortAttribute : Attribute {
        public DescriptionShortAttribute(string description) {
            Description = description;
        }

        public virtual string Description { get; }
    }
}
