using AutoCall.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Helpers {
    public static class FilterHelper {
        public static string LocationRegex = @"\,[\s]+?(?<state>[a-zA-Z]{2})";
        public static string DateSpanRegex = @"^(?<sign>[-+]?)(?<num>[\d]*)(?<abbr>[dwm])$";
        public static string DateSpanFromRegex = @"^(?<sign>[-+]?)(?<num>[\d]*)(?<abbr>[dwm])~$";
        public static string DateSpanToRegex = @"^~(?<sign>[-+]?)(?<num>[\d]*)(?<abbr>[dwm])$";
        public static string DateSpanBetweenRegex = @"^(?<sign1>[-+]?)(?<num1>[\d]*)(?<abbr1>[dwm])~(?<sign2>[-+]?)(?<num2>[\d]*)(?<abbr2>[dwm])$";

        public static string PickupRegex = @"^(pickup|pick|pic)(?<month>[\d]{1,2})\/(?<day>[\d]{1,2})$"; // skipped white spaces
        public static string PickupRangeRegex = @"^(pickup|pick|pic)(?<month1>[\d]{1,2})\/(?<day1>[\d]{1,2})-(?<month2>[\d]{1,2})\/(?<day2>[\d]{1,2})$";// skipped white spaces
        public static string DeliveryRegex = @"^(delivery|del)(?<month>[\d]{1,2})\/(?<day>[\d]{1,2})$";// skipped white spaces
        public static string DeliveryRangeRegex = @"^(delivery|del)(?<month1>[\d]{1,2})\/(?<day1>[\d]{1,2})-(?<month2>[\d]{1,2})\/(?<day2>[\d]{1,2})$";// skipped white spaces
        public static string OriginRegex = @"^(origin|or)\s(?<states>([0-9a-z]{2})+)$";
        public static string DestinationRegex = @"^(destination|destin|dest|des|de)\s(?<states>([0-9a-z]{2})+)$";

        public static Dictionary<string, string> StateToZone { get; set; }

        /// <summary>
        ///     Converts zones to states </summary>
        /// <param name="zones">Example: WAZ1OR</param>
        /// <returns>WADENYPAOR</returns>
        private static string ZonesToStates(string zones) {
            var states = zones;
            foreach (var stateZone in StateToZone) {
                if (states.Contains(stateZone.Value)) {
                    states = states + stateZone.Key;
                }
            }
            foreach (var stateZone in StateToZone) {
                if (states.Split(2).Contains(stateZone.Value)) {
                    states = states.Replace(stateZone.Value, "");
                }
            }
            return states;
        }

        public static bool CheckStates(string city, string filterValue) {
            var match = Regex.Match(city, FilterHelper.LocationRegex);
            return match.Groups["state"].Success && FilterHelper.ZonesToStates(filterValue).ToLower().Split(2).Contains(match.Groups["state"].Value.ToLower());
        }

        public static bool CheckDateSpan(Match match, DateTime date, string value) {
            int sign = 0;
            switch (match.Groups["sign"].Value) {
                case "+":
                    sign = +1;
                    break;
                case "-":
                    sign = -1;
                    break;
                default:
                    sign = +1;
                    break;
            }
            var num = String.IsNullOrEmpty(match.Groups["num"].Value) ? 0 : Int32.Parse(match.Groups["num"].Value);
            switch (match.Groups["abbr"].Value) {
                case "d":
                    return DateTime.Now.Date.AddDays(sign * num) == date.Date;
                case "w":
                    return
                        DateTime.Now.Date.AddDays(sign * num * 7 - (int)DateTime.Now.Date.DayOfWeek) <= date.Date &&
                        DateTime.Now.Date.AddDays(sign * (num + sign) * 7 - (int)DateTime.Now.Date.DayOfWeek) > date.Date;
                case "m":
                    return
                        DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign).AddMonths(sign * num) <= date.Date &&
                        DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign).AddMonths(sign * (num + sign)) > date.Date;
                default:
                    throw new Exception();
            }
        }

        public static bool CheckDateFromSpan(Match match, DateTime date, string value) {
            int sign = 0;
            switch (match.Groups["sign"].Value) {
                case "+":
                    sign = +1;
                    break;
                case "-":
                    sign = -1;
                    break;
                default:
                    sign = +1;
                    break;
            }
            var num = String.IsNullOrEmpty(match.Groups["num"].Value) ? 0 : Int32.Parse(match.Groups["num"].Value);
            switch (match.Groups["abbr"].Value) {
                case "d":
                    return DateTime.Now.Date.AddDays(sign * num) <= date.Date;
                case "w":
                    return
                        DateTime.Now.Date.AddDays(sign * num * 7 - (int)DateTime.Now.Date.DayOfWeek) <= date.Date;
                case "m":
                    return
                        DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign).AddMonths(sign * num) <= date.Date;
                default:
                    throw new Exception();
            }
        }

        public static bool CheckDateToSpan(Match match, DateTime date, string value) {
            int sign = 0;
            switch (match.Groups["sign"].Value) {
                case "+":
                    sign = +1;
                    break;
                case "-":
                    sign = -1;
                    break;
                default:
                    sign = +1;
                    break;
            }
            var num = String.IsNullOrEmpty(match.Groups["num"].Value) ? 0 : Int32.Parse(match.Groups["num"].Value);
            switch (match.Groups["abbr"].Value) {
                case "d":
                    return DateTime.Now.Date.AddDays(sign * num) > date.Date;
                case "w":
                    return
                        DateTime.Now.Date.AddDays(sign * num * 7 - (int)DateTime.Now.Date.DayOfWeek) > date.Date;
                case "m":
                    return
                        DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign).AddMonths(sign * num) > date.Date;
                default:
                    throw new Exception();
            }
        }

        public static bool CheckDateBetweenSpan(Match match, DateTime date, string value) {
            int sign1, sign2 = 0;
            switch (match.Groups["sign1"].Value) {
                case "+":
                    sign1 = +1;
                    break;
                case "-":
                    sign1 = -1;
                    break;
                default:
                    sign1 = +1;
                    break;
            }
            switch (match.Groups["sign2"].Value) {
                case "+":
                    sign2 = +1;
                    break;
                case "-":
                    sign2 = -1;
                    break;
                default:
                    sign2 = +1;
                    break;
            }
            var num1 = String.IsNullOrEmpty(match.Groups["num1"].Value) ? 0 : Int32.Parse(match.Groups["num1"].Value);
            var num2 = String.IsNullOrEmpty(match.Groups["num2"].Value) ? 0 : Int32.Parse(match.Groups["num2"].Value);
            DateTime dt1, dt2;
            switch (match.Groups["abbr1"].Value) {
                case "d":
                    dt1 = DateTime.Now.Date.AddDays(sign1 * num1);
                    break;
                case "w":
                    dt1 = DateTime.Now.Date.AddDays(sign1 * num1 * 7 - (int)DateTime.Now.Date.DayOfWeek);
                    break;
                case "m":
                    dt1 = DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign1).AddMonths(sign1 * num1);
                    break;
                default:
                    throw new Exception();
            }
            switch (match.Groups["abbr2"].Value) {
                case "d":
                    dt2 = DateTime.Now.Date.AddDays(sign2 * num2);
                    break;
                case "w":
                    dt2 = DateTime.Now.Date.AddDays(sign2 * num2 * 7 - (int)DateTime.Now.Date.DayOfWeek);
                    break;
                case "m":
                    dt2 = DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + sign2).AddMonths(sign2 * num2);
                    break;
                default:
                    throw new Exception();
            }

            return dt1 <= date.Date && date.Date <= dt2;
        }

        public static bool CheckPickup(string search, DateTime pickup) {
            search = search.ReplaceAll(" ", "");
            {
                var match = Regex.Match(search, FilterHelper.PickupRegex);
                if (match.Success == true) {
                    var dt = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month"].Value), Int32.Parse(match.Groups["day"].Value));
                    if (dt == pickup.Date) {
                        return true;
                    }
                }
            }
            {
                var match = Regex.Match(search, FilterHelper.PickupRangeRegex);
                if (match.Success == true) {
                    var dt1 = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month1"].Value), Int32.Parse(match.Groups["day1"].Value));
                    var dt2 = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month2"].Value), Int32.Parse(match.Groups["day2"].Value));
                    if (dt1 <= pickup.Date && pickup.Date <= dt2) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckDelivery(string search, DateTime delivery) {
            search = search.ReplaceAll(" ", "");
            {
                var match = Regex.Match(search, FilterHelper.DeliveryRegex);
                if (match.Success == true) {
                    var dt = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month"].Value), Int32.Parse(match.Groups["day"].Value));
                    if (dt == delivery.Date) {
                        return true;
                    }
                }
            }
            {
                var match = Regex.Match(search, FilterHelper.DeliveryRangeRegex);
                if (match.Success == true) {
                    var dt1 = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month1"].Value), Int32.Parse(match.Groups["day1"].Value));
                    var dt2 = new DateTime(DateTime.Now.Year, Int32.Parse(match.Groups["month2"].Value), Int32.Parse(match.Groups["day2"].Value));
                    if (dt1 <= delivery.Date && delivery.Date <= dt2) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckOrigin(string search, string origin) {
            var match = Regex.Match(search, FilterHelper.OriginRegex);
            if (match.Success == true) {
                var states = match.Groups["states"].Value;
                return CheckStates(origin, states.ToUpper());
            }
            return false;
        }

        public static bool CheckDestination(string search, string destination) {
            var match = Regex.Match(search, FilterHelper.DestinationRegex);
            if (match.Success == true) {
                var states = match.Groups["states"].Value;
                return CheckStates(destination, states.ToUpper());
            }
            return false;
        }
    }
}
