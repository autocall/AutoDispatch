using System.Collections.Generic;
using AutoCall.Logic.Settings.Sections;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;

namespace AutoDispatch.Logic.Converters {
    /// <summary> Именованные фильтры борда (V1 Settings/FiltersColumns/FiltersColumns_SettingsToVm, FiltersColumns_VmToSettings) </summary>
    public static class FiltersColumnsConverter {
        public static FiltersColumnsViewModel ToVm(FiltersColumnsSettings source) {
            if (source == null) return null;
            var destination = new FiltersColumnsViewModel() {
                Table = (DispatchFilterTableType)source.Table
            };
            foreach(var filter in source.Filters) {
                var filterVm = new FilterColumnsViewModel() {
                    Name = filter.Name
                };
                foreach (var column in filter.Columns) {
                    filterVm.Columns.Add(new FilterColumnViewModel((FilterColumnEnum)column.Key, column.Value));
                }
                destination.Filters.Add(filterVm);
            }

            return destination;
        }
        public static FiltersColumnsSettings ToSettings(FiltersColumnsViewModel source) {
            if (source == null) return null;
            var destination = new FiltersColumnsSettings() {
                Table = (byte)source.Table,
                Filters = new List<FilterColumnsSettings>()
            };
            foreach (var filter in source.Filters) {
                var filterVm = new FilterColumnsSettings() {
                    Name = filter.Name,
                    Columns = new Dictionary<byte, string>()
                };
                foreach (var column in filter.Columns) {
                    filterVm.Columns.Add((byte)column.Column, column.Value);
                }
                destination.Filters.Add(filterVm);
            }

            return destination;
        }
    }
}