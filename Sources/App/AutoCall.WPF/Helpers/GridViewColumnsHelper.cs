using AutoCall.Common.Collections;
using AutoCall.Logic.Settings.Sections;
using AutoCall.WPF.Extensions;
using AutoCall.WPF.UserControls.Boards;
using AutoDispatch.Logic.Enums;
using AutoDispatch.Logic.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Telerik.Windows.Controls;
using AutoCall.WPF.Helpers;
using AutoCall.Logic.Contracts.Enums;
using AutoCall.WPF.UserControls;
using System.Diagnostics;

namespace AutoCall.WPF.Helpers {
    /// <summary>
    ///     Synchronization of all columns </summary>
    public static class GridViewColumnsHelper {
        public static void LoadAndBindSettingsViewModel(
                IMapper mapper,
                SettingsTableNames table,
                GridViewSettings settings,
                RadGridView grid,
                GridsSettingsViewModel model) {
            //Console.WriteLine($" - GridViewColumnsHelper.LoadAndBindSettingsViewModel({table})");

            var settingsViewModel = mapper.Map<GridSettingsViewModel>(settings ?? new GridViewSettings());
            model.Grids[table] = settingsViewModel;

            GridSettingsViewModel gridSettingsViewModel;
            if (model.Grids.ContainsKey(table)) {
                gridSettingsViewModel = model.Grids[table];
            } else {
                gridSettingsViewModel = new GridSettingsViewModel();
                model.Grids.Add(table, gridSettingsViewModel);
            }

            var columnIndexes = new List<int>();

            foreach (GridViewColumn column in grid.Columns) {
                if (column.UniqueName == "Id" ||
                column.UniqueName == "CreditCheck")
                // V1: || (column.UniqueName == "DispatchRating" && …FindVisualParent2<DatPowerBoard>().BoardType == BoardType.DatPowerPostTrucks) —
                // колонка DispatchRating только у бордов DatPower (модуль AutoCall)
                    continue;
                GridColumnSettingsViewModel columnModel = gridSettingsViewModel.Columns.FirstOrDefault(e => e.UniqueName == column.UniqueName);
                if (columnModel == null) {
                    columnModel = new GridColumnSettingsViewModel() {
                        UniqueName = column.UniqueName,
                        DisplayIndex = column.DisplayIndex,
                        Width = column.Width,
                        IsVisible = column.IsVisible,
                    };
                    gridSettingsViewModel.Columns.Add(columnModel);
                }

            l1:
                if (columnIndexes.Contains(columnModel.DisplayIndex)) {
                    columnModel.DisplayIndex++;
                    goto l1;
                } else {
                    columnIndexes.Add(columnModel.DisplayIndex);
                }

                column.SetBinding(GridViewColumn.DisplayIndexProperty, new Binding(nameof(columnModel.DisplayIndex)) {
                    Source = columnModel,
                    Mode = BindingMode.TwoWay
                });
                if (column.IsResizable) column.SetBinding(GridViewColumn.WidthProperty, new Binding(nameof(columnModel.Width)) {
                    Source = columnModel,
                    Mode = BindingMode.TwoWay
                });
                column.SetBinding(GridViewColumn.IsVisibleProperty, new Binding(nameof(columnModel.IsVisible)) {
                    Source = columnModel,
                    Mode = BindingMode.TwoWay
                });
            }
        }

        public static void SyncChildColumn(
                GridViewColumn parentColumn,
                GridSettingsViewModel parentGridViewSettingsViewModel,
                GridViewColumnCollection childColumns) {

            GridViewColumn childColumn = (GridViewColumn)Activator.CreateInstance(parentColumn.GetType());
            if (parentColumn is SumDataColumn c1 && childColumn is SumDataColumn c2) {
                c2.SumMemberPath = c1.SumMemberPath;
            }
            childColumn.CopyPropertiesFrom(parentColumn);
            childColumn.Header = null;
            childColumn.IsReorderable = true;
            childColumn.IsResizable = true;
            childColumn.CellEditTemplate = parentColumn.CellEditTemplate;
            childColumn.CellStyle = parentColumn.CellStyle;
            childColumns.Add(childColumn);

            var columnModel = parentGridViewSettingsViewModel.Columns.First(e => e.UniqueName == parentColumn.UniqueName);

            // don't use! buggy
            //childColumn.SetBinding(GridViewColumn.WidthProperty, new Binding("ActualWidth") { Source = parentColumn, Mode = BindingMode.OneWay });
            //childColumn.SetBinding(GridViewColumn.IsVisibleProperty, new Binding("IsVisible") { Source = parentColumn, Mode = BindingMode.OneWay });
            //newColumn.SetBinding(GridViewColumn.DisplayIndexProperty, new Binding("DisplayIndex") { Source = column, Mode = BindingMode.OneWay });
            // don't use! buggy
            //newColumn.SetBinding(GridViewColumn.WidthProperty, new Binding(nameof(columnModel.Width)) { Source = columnModel, Mode = BindingMode.TwoWay/*OneWay was not worked*/ });
            //newColumn.SetBinding(GridViewColumn.IsVisibleProperty, new Binding(nameof(columnModel.IsVisible)) { Source = columnModel, Mode = BindingMode.OneWay });
            //newColumn.SetBinding(GridViewColumn.DisplayIndexProperty, new Binding(nameof(columnModel.DisplayIndex)) { Source = columnModel, Mode = BindingMode.OneWay });

            // uses actual size
            childColumn.Width = columnModel.Width.IsAuto == false ? columnModel.Width : parentColumn.ActualWidth;
            childColumn.IsVisible = columnModel.IsVisible;
            childColumn.DisplayIndex = columnModel.DisplayIndex;

            /******* SYNCS SUBTABLE COLUMNS *******/
            columnModel.PropertyChanged += (sender2, args2) => {
                if (args2.PropertyName == nameof(columnModel.Width)) {
                    childColumn.Width = columnModel.Width.IsAuto == false ? columnModel.Width : parentColumn.ActualWidth;
                }
                if (args2.PropertyName == nameof(columnModel.IsVisible)) {
                    childColumn.IsVisible = columnModel.IsVisible;
                }
                if (args2.PropertyName == nameof(columnModel.DisplayIndex)) {
                    childColumn.DisplayIndex = columnModel.DisplayIndex;
                }
            };
        }

        /// <summary>
        ///     Reorders of column group </summary>
        public static void ReorderGroup(RadGridView gridView, ColumnReorderingEventArgs args) {
            //Console.WriteLine($" - GridViewColumnsHelper.ReorderGroup({gridView.Name})");
            // NOTES: Expenses/Accessorial has any position
            //if ((PropertyExtensions.GetRelativeGroup(args.Column)?.StartsWith("Expenses") ?? false) ||
            //    (PropertyExtensions.GetRelativeGroup(args.Column)?.StartsWith("Accessorial") ?? false) ||
            //    (PropertyExtensions.GetRelativeGroup(args.Column)?.StartsWith("CallPhone") ?? false)) {
            //    return;
            //}
            // ordered columns
            List<GridViewColumn> columns = gridView.Columns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex).ToList();
            // fixes new index
            var newDisplayIndex = args.NewDisplayIndex;
            if (args.Column.DisplayIndex > args.NewDisplayIndex) {
                newDisplayIndex--;
            } else {
                newDisplayIndex++;
            }
            // simple reordering
            if ((newDisplayIndex - 1 >= 0 && GetGroupName(columns[newDisplayIndex - 1]) == GetGroupName(args.Column)) ||
                (newDisplayIndex + 1 < columns.Count && GetGroupName(columns[newDisplayIndex + 1]) == GetGroupName(args.Column))) {
                args.Column.DisplayIndex = args.NewDisplayIndex;
                return;
            }

            // reordering columns
            List<GridViewColumn> groupColumns = columns.Where(e => GetGroupName(e) == GetGroupName(args.Column)).ToList();
            // reverses list
            if (args.Column.DisplayIndex > args.NewDisplayIndex) {
                groupColumns.Reverse();
            } else {
                args.NewDisplayIndex++;
            }

            // reorders columns
            foreach (var column in groupColumns) {
                columns.Move(column, args.NewDisplayIndex);
            }
            // sets DisplayIndex
            for (int i = 0; i < columns.Count; i++) {
                //if (columns[i].DisplayIndex != i) {
                //    Console.WriteLine($" - ReorderGroup > {columns[i].Name} {columns[i].DisplayIndex} -> {i}");
                //}
                columns[i].DisplayIndex = i;
            }
        }

        /// <summary>
        ///     A order within the group </summary>
        public static void ReorderGroupRelative(RadGridView gridView) {
            //Console.WriteLine($" - GridViewColumnsHelper.ReorderGroupRelative({gridView.Name})");
            // ordered columns
            List<GridViewColumn> columns = gridView.Columns.Cast<GridViewColumn>().OrderBy(e => e.DisplayIndex).ToList();

            var groups = columns.Cast<GridViewColumn>().GroupBy(e => GetGroupName(e));
            foreach (var group in groups) {
                var minColumn = group.First();
                var relativeColumns = group
                    .OrderByDescending(e => PropertyExtensions.GetRelativeDisplayIndex(e) ?? e.DisplayIndex + 100)
                    .ToList();
                foreach (var relativeColumn in relativeColumns) {
                    if ((PropertyExtensions.GetRelativeGroup(relativeColumn) ?? "").StartsWith("Expenses") ||
                        (PropertyExtensions.GetRelativeGroup(relativeColumn) ?? "").StartsWith("Accessorial") ||
                        (PropertyExtensions.GetRelativeGroup(relativeColumn) ?? "") == "CallPhone") {
                        columns.Move(relativeColumn, minColumn.DisplayIndex > 0 ? minColumn.DisplayIndex : 0);
                    }
                }
                // sets DisplayIndex
                for (int i = 0; i < columns.Count; i++) {
                    //if (columns[i].DisplayIndex != i) {
                    //    Console.WriteLine($" - ReorderGroup > {columns[i].Name} {columns[i].DisplayIndex} -> {i}");
                    //}
                    columns[i].DisplayIndex = i;
                }
            }
        }

        private static string GetGroupName(GridViewColumn column) {
            return PropertyExtensions.GetRelativeGroup(column);
        }
    }
}
