using AutoCall.Common.Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AutoDispatch.Logic.Heplers {
    public static class ScanFileHelper {

        // мёртвый код V1 (AI/wiki/plan.md «Сканер TWAIN в App», решено 2026-11-18): RenameTempFile нигде не вызывается, поэтому файлов
        // trailer_scheme_{номер}_{N}.pdf нет и FindFile всегда возвращает null
//        // TODO операции с файлами вынеси в Helper. Добавится операция поиска файла, удаления
//        public static void RenameTempFile(string trailer, string filePath) {
//            var trailerNumber = StringExtension.OnlyNumbersAndLetters(trailer).ToLower();
//            if (String.IsNullOrEmpty(trailerNumber) == false) {
//                var files = Directory.GetFiles(Path.GetTempPath(), $"trailer_scheme_{trailerNumber}_*.pdf");
//                Regex regex = new Regex($@"^trailer_scheme_{trailerNumber}_(?<num>[\d]+)\.pdf$");
//                var index = 0;
//                foreach (var fullPath in files) {
//                    var file = Path.GetFileName(fullPath);
//                    if (regex.IsMatch(file)) {
//                        index = Math.Max(index, Int32.Parse(regex.Match(file).Groups["num"].Value));
//                    }
//                }
//                var newTempFileName = $"trailer_scheme_{trailerNumber}_{index + 1}.pdf";
//                File.Move(filePath, Path.Combine(Path.GetTempPath(), newTempFileName));
//            }
//        }
//
//        public static string FindFile(string trailer) {
//            var trailerNumber = StringExtension.OnlyNumbersAndLetters(trailer).ToLower();
//            if (String.IsNullOrEmpty(trailerNumber) == false) {
//                var files = Directory.GetFiles(Path.GetTempPath(), $"trailer_scheme_{trailerNumber}_*.pdf");
//                Regex regex = new Regex($@"^trailer_scheme_{trailerNumber}_(?<num>[\d]+)\.pdf$");
//                var index = 0;
//                var indexString = String.Empty;
//                foreach (var fullPath in files) {
//                    var file = Path.GetFileName(fullPath);
//                    if (regex.IsMatch(file)) {
//                        indexString = regex.Match(file).Groups["num"].Value;
//                        index = Math.Max(index, Int32.Parse(indexString));
//                    }
//                }
//                if (String.IsNullOrEmpty(indexString) == false) {
//                    var fileName = $"trailer_scheme_{trailerNumber}_{indexString}.pdf";
//                    return Path.Combine(Path.GetTempPath(), fileName);
//                }
//            }
//            return null;
//        }

        public static void DeleteOldTempFiles() {
            var files = Directory.GetFiles(Path.GetTempPath(), $"trailer_scheme_*.pdf");
            foreach (var file in files) {
                try {
                    if (DateTime.UtcNow - File.GetCreationTimeUtc(file) > TimeSpan.FromDays(1)) {
                        File.Delete(file);
                    }
                } catch (Exception) {
                }
            }
        }
    }
}
