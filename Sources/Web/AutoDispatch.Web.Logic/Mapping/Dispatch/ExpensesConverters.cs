using Newtonsoft.Json.Linq;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Logic.Dtos.Dispatch;

namespace AutoDispatch.Web.Logic.Mapping.Dispatch;

/// <summary>
///     Load.Expenses/Gains (JSON view-моделей клиента) → списки по всем типам (V1 Expenses_DtoToVm/Gains_DtoToVm).
///     Пустой или null JSON → список всех типов без сумм, как в V1 </summary>
public static class ExpensesConverter {

    public static List<GainViewModel> ToGains(string json) {
        var gains = Deserialize<GainViewModel>(json, "Gains");
        return Enum.GetValues(typeof(GainType)).Cast<GainType>()
            .Select(t => gains.FirstOrDefault(e => e.Type == t) ?? new GainViewModel() { Type = t }).ToList();
    }

    // V1 App пишет массив с $type клиента (GainViewModel[], AutoDispatch.Logic.Contracts, TypeNameHandling.All): $type не разрешается,
    // а пропускается — элементы берутся из $values по именам полей. Подмена имён сборок на серверные (как в V1) после этапа 4
    // не проходит whitelist MscorlibBinder (AutoDispatch.Web.Logic в $type запрещён) — найдено на /Confirmation 2026-11-17
    private static T[] Deserialize<T>(string json, string name) {
        if (String.IsNullOrEmpty(json)) return new T[0];
        try {
            var token = JToken.Parse(json);
            var values = token is JObject obj ? obj["$values"] : token;
            return values?.ToObject<T[]>() ?? new T[0];
        } catch (Exception e) {
            _l.e($"Deserialize {name}", e);
            return new T[0];
        }
    }
}
