using System;
using System.Collections.Generic;
using System.Linq;
using AutoCall.Common;
using AutoCall.Common.Attributes;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;

namespace AutoDispatch.Logic.Converters {
    /// <summary>
    ///     Расходы груза: JSON строки LoadDto.Expenses ↔ список ExpenseViewModel по всем ExpenseType (V1 Expenses_DtoToVm/Expenses_VmToDto).
    ///     Статический конвертер (AI/wiki/plan.md «DI и AutoMapper в App»); лежит в Contracts, а не в AutoDispatch.Logic/Converters —
    ///     его вызывает конструктор LoadViewModel (V1 — через ServiceLocator и IMapper) </summary>
    public static class ExpensesConverter {

        public static List<ExpenseViewModel> ToVm(string json) {
            ExpenseViewModel[] expenses = new ExpenseViewModel[0];
            if (String.IsNullOrEmpty(json) == false) {
                try {
                    expenses = JsonExtension.DeserializeObject<ExpenseViewModel[]>(json);
                } catch (Exception e) {
                    _webl.e("Deserialize Expenses", e);
                }
            }

            return new List<ExpenseViewModel>(Enum.GetValues(typeof(ExpenseType)).Cast<ExpenseType>()
                .Where(x => DisplayAttribute.HasDisplay(x))
                .OrderBy(x => DisplayAttribute.GetOrder(x))
                .Select(t => expenses.FirstOrDefault(e => e.Type == t) ?? new ExpenseViewModel() { Type = t }).ToList());
        }

        public static string ToDto(List<ExpenseViewModel> source) {
            if (source == null) return null; // V1 AutoMapper TypeConverter: null-источник → null без вызова конвертера (части после Split)
            if (source.Any()) {
                return JsonExtension.SerializeObject(source.Where(e => e.Cost != null).ToArray());
            } else {
                return null;
            }
        }
    }

    /// <summary>
    ///     Доплаты груза: JSON строки LoadDto.Gains ↔ список GainViewModel по всем GainType (V1 Gains_DtoToVm/Gains_VmToDto) </summary>
    public static class GainsConverter {

        public static List<GainViewModel> ToVm(string json) {
            GainViewModel[] gains = new GainViewModel[0];
            if (String.IsNullOrEmpty(json) == false) {
                try {
                    gains = JsonExtension.DeserializeObject<GainViewModel[]>(json);
                } catch (Exception e) {
                    _webl.e("Deserialize Gains", e);
                }
            }

            return new List<GainViewModel>(Enum.GetValues(typeof(GainType)).Cast<GainType>()
                .Where(x => DisplayAttribute.HasDisplay(x))
                .OrderBy(x => DisplayAttribute.GetOrder(x))
                .Select(t => gains.FirstOrDefault(e => e.Type == t) ?? new GainViewModel() { Type = t }).ToList());
        }

        public static string ToDto(List<GainViewModel> source) {
            if (source == null) return null; // V1 AutoMapper TypeConverter: null-источник → null без вызова конвертера
            if (source.Any()) {
                return JsonExtension.SerializeObject(source.Where(e => e.Cost != null).ToArray());
            } else {
                return null;
            }
        }
    }
}
