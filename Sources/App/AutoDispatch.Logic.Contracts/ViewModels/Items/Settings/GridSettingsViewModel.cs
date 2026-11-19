using AutoDispatch.Logic.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Controls;

namespace AutoDispatch.Logic.ViewModels {
    public class GridsSettingsViewModel {
        public Dictionary<SettingsTableNames, GridSettingsViewModel> Grids { get; set; } = new Dictionary<SettingsTableNames, GridSettingsViewModel>();

        /// <summary>
        ///     Selects Dispatch or DatPower grids </summary>
        private List<GridSettingsViewModel> SelectGrids(GridColumnSettingsViewModel viewModel) {
            foreach (var grid in Grids) {
                foreach (var column in grid.Value.Columns) {
                    if (column == viewModel) {
                        return SelectGrids(grid.Key);
                    }
                }
            }
            throw new Exception();
        }

        /// <summary>
        ///     List of table without parameter </summary>
        private List<GridSettingsViewModel> SelectGrids(SettingsTableNames tableName) {
            var datPowerTables = new List<SettingsTableNames>() {
                SettingsTableNames.SearchLoads_Patterns,
                SettingsTableNames.SearchLoads_Records,
                SettingsTableNames.SearchTrucks_Patterns,
                SettingsTableNames.SearchTrucks_Records,
                SettingsTableNames.PostLoads_Patterns,
                SettingsTableNames.PostLoads_Records,
                SettingsTableNames.PostTrucks_Patterns,
                SettingsTableNames.PostTrucks_Records
            };

            var dispatchTables = new List<SettingsTableNames>() {
                SettingsTableNames.Carrier_Patterns,
                SettingsTableNames.Carrier_Records,
                SettingsTableNames.Broker_Patterns,
                SettingsTableNames.Broker_Records,
                SettingsTableNames.Accounting_Carriers,
                SettingsTableNames.Accounting_Brokers,
                SettingsTableNames.History_Carriers
            };

            if (datPowerTables.Contains(tableName)) {
                return Grids.Where(e => datPowerTables.Contains(e.Key) && e.Key != tableName).Select(e => e.Value).ToList();
            }
            if (dispatchTables.Contains(tableName)) {
                return Grids.Where(e => dispatchTables.Contains(e.Key) && e.Key != tableName).Select(e => e.Value).ToList();
            }
            throw new Exception();
        }

    }

    public class GridSettingsViewModel {
        public List<GridColumnSettingsViewModel> Columns { get; set; } = new List<GridColumnSettingsViewModel>();
        public string SelectedFilterColumnsName { get; set; }
    }

    public class GridColumnSettingsViewModel : ViewModel {

        public Action<GridColumnSettingsViewModel> SyncProperties;

        private GridViewLength _width;
        private int _displayIndex;
        private bool _isVisible = true;

        public string UniqueName { get; set; }

        public GridViewLength Width {
            get => _width;
            set {
                // NOTES: I don't use Auto Width
                if (_width != value && !(_width.IsAuto && value.IsAuto) && !(_width.IsSizeToCells && value.IsSizeToCells)) {
                    _width = value;
                    base.OnPropertyChanged();
                    SyncProperties?.Invoke(this);
                }
            }
        }

        public int DisplayIndex {
            get => _displayIndex;
            set {
                if (_displayIndex != value) {
                    _displayIndex = value;
                    base.OnPropertyChanged();
                    SyncProperties?.Invoke(this);
                }
            }
        }

        public bool IsVisible {
            get => _isVisible;
            set {
                if (_isVisible != value) {
                    _isVisible = value;
                    base.OnPropertyChanged();
                    SyncProperties?.Invoke(this);
                }
            }
        }
    }
}
