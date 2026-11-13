using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common.Extensions;
using AutoDispatch.Web.Data.Entities.Dispatch;

namespace AutoDispatch.Web.Logic.Helpers.Dispatch;

/// <summary>
///     Кастомизации нумерации компании (V1 CompanyCustomizationEnum): DispatchCompanyCustom.Type </summary>
public enum CompanyCustomizationEnum {
    None = 0,
    ModifyIndexByLocation = 1,
    PrefixIndex = 2
}

/// <summary>
///     Нумерация грузов и инвойсов, ставка груза (V1 AutoCall.Logic.Helpers.DispatchHelper). ActualCompany → DispatchCompany </summary>
public static class DispatchHelper {
    public static string LocationRegex = @"\,[\s]+?(?<state>[a-zA-Z]{2})";

    /// <summary>
    ///     Следующий номер груза; меняет NextLoadNumber/NextLoadExtra компании — сохранить компанию должен вызывающий </summary>
    public static int? NextLoadNumber(LoadDto load, DispatchCompany company, ICollection<DispatchCompanyCustom> customizes) {
        DispatchCompanyCustom customize;
        customizes = customizes ?? new List<DispatchCompanyCustom>();
        if ((customize = customizes.FirstOrDefault(e => e.Type == (int)CompanyCustomizationEnum.ModifyIndexByLocation)) != null) {
            // Valids Params
            if (Regex.IsMatch(customize.Params, @"^([\w]+:[\w]+[;]?)+$") == false) {
                throw new Exception("customization not configured");
            }
            // Converts Params to Dictionary[State -> abbr]
            Dictionary<string, string> states = customize.Params.Split(';').ToDictionary(e => e.Split(':')[0].ToLower(), e => e.Split(':')[1]);
            var statesArr = states.Select(e => e.Key).ToList();
            // Recognizes State from Origin
            var originStateMatch = Regex.Match(load.OriginCity.ToLower(), LocationRegex);
            // Regex State
            var destinationStateMatch = Regex.Match(load.DestinationCity.ToLower(), LocationRegex);
            // Checks State
            if (originStateMatch.Success && states.ContainsKey(originStateMatch.Groups["state"].Value)) {
                var i = statesArr.IndexOf(originStateMatch.Groups["state"].Value);
                load.IndexExtra = states[originStateMatch.Groups["state"].Value];
                int[] indexes = BinaryHelper.BytesToInts(company.NextLoadExtra, states.Count + 1);
                indexes[i]++;
                company.NextLoadExtra = BinaryHelper.IntsToBytes(indexes, states.Count + 1);
                return indexes[i];
            } else
            if (destinationStateMatch.Success && states.ContainsKey(destinationStateMatch.Groups["state"].Value)) {
                var i = statesArr.IndexOf(destinationStateMatch.Groups["state"].Value);
                load.IndexExtra = states[destinationStateMatch.Groups["state"].Value];
                int[] indexes = BinaryHelper.BytesToInts(company.NextLoadExtra, states.Count + 1);
                indexes[i]++;
                company.NextLoadExtra = BinaryHelper.IntsToBytes(indexes, states.Count + 1);
                return indexes[i];
            } else {
                // undefined
                load.IndexExtra = "?";
                int[] indexes = BinaryHelper.BytesToInts(company.NextLoadExtra, states.Count + 1);
                indexes[states.Count]++;
                company.NextLoadExtra = BinaryHelper.IntsToBytes(indexes, states.Count + 1);
                return indexes[states.Count];
            }
        } else if ((customize = customizes.FirstOrDefault(e => e.Type == (int)CompanyCustomizationEnum.PrefixIndex)) != null) {
            load.IndexPrefix = customize.Params;
            load.Index = company.NextLoadNumber++;
            return company.NextLoadNumber;
        } else {
            load.Index = company.NextLoadNumber++;
            return company.NextLoadNumber;
        }
    }

    public static string NextInvoiceNumber(LoadDto load, DispatchCompany company, ICollection<DispatchCompanyCustom> customizes) {
        DispatchCompanyCustom customize;
        customizes = customizes ?? new List<DispatchCompanyCustom>();
        if ((customize = customizes.FirstOrDefault(e => e.Type == (int)CompanyCustomizationEnum.PrefixIndex)) != null) {
            company.NextInvoiceNumber++;
            load.InvoiceNumber = customize.Params + company.NextInvoiceNumber;
            return load.InvoiceNumber;
        } else {
            company.NextInvoiceNumber++;
            load.InvoiceNumber = company.NextInvoiceNumber.ToString();
            return load.InvoiceNumber;
        }
    }

    /// <summary>
    ///     Gets Rate + Total Gains </summary>
    public static decimal? GetRate(LoadDto load) {
        try {
            decimal totalGains = load.CarrierViewTracking ?? 0;
            foreach (var value in ((dynamic)JObject.Parse(load.Gains))["$values"]) {
                totalGains += Convert.ToDecimal(((JValue)value.Cost).Value);
            }
            return load.LineHaul + totalGains;
        } catch (Exception) {
            return load.LineHaul;
        }
    }

    /// <summary>
    ///     Gets Rate + Total Gains - Total Expenses </summary>
    public static decimal GetRate2(Load load) {
        return (load.CarrierViewTracking ?? 0) + (load.LineHaul ?? 0) + GetGains(load) - GetExpences(load);
    }

    private static decimal GetGains(Load load) {
        try {
            decimal totalGains = 0;
            foreach (var value in (dynamic)JsonConvert.DeserializeObject(load.Gains)) {
                totalGains += Convert.ToDecimal(value.Cost.Value);
            }
            return totalGains;
        } catch (Exception) {
            return 0;
        }
    }

    private static decimal GetExpences(Load load) {
        try {
            decimal totalExpences = 0;
            foreach (var value in (dynamic)JsonConvert.DeserializeObject(load.Expenses)) {
                totalExpences += Convert.ToDecimal(value.Cost.Value);
            }
            return totalExpences;
        } catch (Exception) {
            return 0;
        }
    }
}
