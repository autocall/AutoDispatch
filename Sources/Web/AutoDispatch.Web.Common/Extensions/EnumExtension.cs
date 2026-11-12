using System.ComponentModel;
using System.Reflection;

namespace AutoDispatch.Web.Common.Extensions;

public static class EnumExtension {

    /// <summary>
    ///     Текст из [Description] либо имя значения </summary>
    public static string GetDescription(this Enum value) {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
