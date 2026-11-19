using System;
using System.Globalization;

namespace AutoCall.Common.Helpers {
    public static class DispatchHelper {
        public static string DateTimeToAppointment(DateTime date1, DateTime date2) {
            if (date1 == date2) {
                return String.Format("{0:M/d} App {0:h:mmtt}", date1);
            }
            if (date1.Date == date2.Date) {
                return DateTimeToAppointment(date1, date1.TimeOfDay, date2.TimeOfDay);
            } else {
                return String.Format("{0:M/d} {0:h:mmtt} - {1:M/d} {1:h:mmtt} ", date1, date2);
            }
        }
        public static string DateTimeToAppointment(DateTime date, TimeSpan time1, TimeSpan time2) {
            if (time1 == time2) {
                return String.Format("{0:M/d} App {1:h:mmtt}", date, date.Date + time1);
            } else {
                return String.Format("{0:M/d} {1:h:mmtt} - {2:h:mmtt}", date, date.Date + time1, date.Date + time2);
            }
        }
        public static string DateTimeToAppointmentFullDateTime(DateTime date, TimeSpan time1, TimeSpan time2) {
            if (time1 == time2) {
                return String.Format("{0:M/d/yyyy} App {1:h:mmtt}", date, date.Date + time1);
            } else {
                return String.Format("{0:M/d/yyyy} {1:h:mmtt} - {2:h:mmtt}", date, date.Date + time1, date.Date + time2);
            }
        }

        // do not use. use local method with WeightTypeEnum
        //public static string NullableWeightToString(this int? value) {
        //    if (value.HasValue) return $"{value} kLbs";
        //    return "";
        //}

        public static string NullableLengthToString(this int? value) {
            if (value.HasValue) return $"{value} ft";
            return "";
        }
    }
}
