using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using AutoDispatch.Dtos.Enums;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoCall.WPF.Helpers {
    public static class DispatchHelper {

        /// <summary>
        ///     V1 — Xceed.Wpf.Toolkit.MessageBox (Extended WPF Toolkit) с кнопками Yes = «Pallets», No = «Length», Cancel; пакет не переносится
        ///     (MS-PL — только .NET Framework), тот же вопрос — TaskDialog WinForms </summary>
        public static DivideType AskDivideType(Window window) {
            var pallets = new System.Windows.Forms.TaskDialogButton("Pallets");
            var length = new System.Windows.Forms.TaskDialogButton("Length");
            var page = new System.Windows.Forms.TaskDialogPage() {
                Caption = "Loading type",
                Text = "How do you load to the trailer?",
                Icon = System.Windows.Forms.TaskDialogIcon.Information,
                Buttons = { pallets, length, System.Windows.Forms.TaskDialogButton.Cancel },
            };
            var owner = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var result = System.Windows.Forms.TaskDialog.ShowDialog(owner, page);
            if (result == pallets) {
                return DivideType.Pallets;
            }
            if (result == length) {
                return DivideType.Length;
            }
            return DivideType.None;
        }

        public static int? GetMc(int value) {
            return value < 999999 ? (int?)value : null;
        }

        public static int? GetDot(int value) {
            return value > 999999 ? (int?)value : null;
        }

        /// <summary>
        ///     Parses MC </summary>
        /// <example>FF-22664\r\n\rMC-415186</example>
        public static int ParseMcNumber(string value) {
            var match = Regex.Match(value, @"MC-(?<value>\d+)");
            if (match.Success) {
                return Int32.Parse(match.Groups["value"].Value);
            } else {
                throw new Exception("Undefined MC#");
            }
        }

        internal static bool TryParseMcNumber(string value, out int mcNumber) {
            var match = Regex.Match(value, @"MC-(?<value>\d+)");
            if (match.Groups["value"] != null) {
                return Int32.TryParse(match.Groups["value"].Value, out mcNumber);
            } else {
                mcNumber = 0;
                return false;
            }
        }

        #region [ AutoCompleter ]

        internal static List<DatCompanyViewModel> SearchDatCompanies(List<DatCompanyViewModel> datCompanies,
            Guid? actualCompanyId, string searchText, DatCompanyType type) {
            return datCompanies.Where(e => e.ActualCompanyId == actualCompanyId && e.EntityType == type &&
                                           e.Name.Trim().ToLower().Contains(searchText.Trim().ToLower())).ToList();
        }

        internal static List<DatCompanyViewModel> MergeLists(List<DatCompanyViewModel> censusList, List<DatCompanyViewModel> brokerList) {
            foreach (var broker in brokerList.OrderByDescending(e => e.CreatedDate)) {
                var census = censusList.FirstOrDefault(e => (broker.DotNumber != null && e.DotNumber == broker.DotNumber) || 
                                                            (broker.McNumber != null && e.McNumber == broker.McNumber));
                if (census != null) {
                    census.ActualCompanyId = broker.ActualCompanyId;
                    census.Contacts = broker.Contacts;
                } else {
                    censusList.Insert(0, broker);
                }
            }
            return censusList;
        }

        #endregion [ AutoCompleter ]
    }
}
