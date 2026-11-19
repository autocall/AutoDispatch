using AutoCall.Common.Extensions;
using System;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Heplers {
    public static class FileHelper {
        public static string IncFileName(string fileName) {
            var num = 0;
            var match = Regex.Match(fileName, @"(?<num>\([\d]+\)).[\d\w]+$");
            if (match.Success) {
                num = Int32.Parse(StringExtension.OnlyNumbers(match.Groups["num"].Value));
           return fileName.Replace(match.Groups["num"].Value, $"({++num})");
            } else {
                if (fileName.Contains(".")) {
                    var arr = StringExtension.SplitLast(fileName, '.');
                    return arr[0] + "(1)." + arr[1];
                } else {
                    return fileName + "(1)";
                }
            }
        }
    }
}
