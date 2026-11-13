using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Dtos.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

// Load.Expenses/Gains (JSON view-моделей клиента) → списки по всем типам (V1 Expenses_DtoToVm/Gains_DtoToVm).
// $type клиента (AutoDispatch.Logic.Contracts) подменяется на серверные типы, как в V1

public class Expenses_DtoToVm : TypeConverter<string, List<ExpenseViewModel>> {
    protected override List<ExpenseViewModel> ConvertCore(string json) {
        var expenses = Deserialize<ExpenseViewModel>(json, "Expenses");
        return Enum.GetValues(typeof(ExpenseType)).Cast<ExpenseType>()
            .Select(t => expenses.FirstOrDefault(e => e.Type == t) ?? new ExpenseViewModel() { Type = t }).ToList();
    }

    internal static T[] Deserialize<T>(string json, string name) {
        if (String.IsNullOrEmpty(json)) return new T[0];
        try {
            var serverType = typeof(T);
            json = json
                .Replace($"AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses.{serverType.Name}, AutoDispatch.Logic.Contracts", $"{serverType.FullName}, {serverType.Assembly.GetName().Name}")
                .Replace(", AutoDispatch.Logic.Contracts", $", {serverType.Assembly.GetName().Name}");
            return DispatchJson.Deserialize<T[]>(json) ?? new T[0];
        } catch (Exception e) {
            _l.e($"Deserialize {name}", e);
            return new T[0];
        }
    }
}

public class Gains_DtoToVm : TypeConverter<string, List<GainViewModel>> {
    protected override List<GainViewModel> ConvertCore(string json) {
        var gains = Expenses_DtoToVm.Deserialize<GainViewModel>(json, "Gains");
        return Enum.GetValues(typeof(GainType)).Cast<GainType>()
            .Select(t => gains.FirstOrDefault(e => e.Type == t) ?? new GainViewModel() { Type = t }).ToList();
    }
}
