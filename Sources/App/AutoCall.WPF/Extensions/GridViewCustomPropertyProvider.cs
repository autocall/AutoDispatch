using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using Telerik.Windows.Persistence.Services;

namespace AutoCall.WPF.Extensions {
    public class GridViewCustomPropertyProvider : ICustomPropertyProvider {
        public CustomPropertyInfo[] GetCustomProperties() {
            // Create three custom properties to persist the Columns, Sorting and Group descriptors using proxy objects
            return new CustomPropertyInfo[] {
                new CustomPropertyInfo("Columns", typeof(List<ColumnProxy>)),
                new CustomPropertyInfo("SortDescriptors", typeof(List<SortDescriptorProxy>)),
                new CustomPropertyInfo("GroupDescriptors", typeof(List<GroupDescriptorProxy>)),
                new CustomPropertyInfo("FilterDescriptors", typeof(List<FilterSetting>)),
            };
        }

        public void InitializeObject(object context) {
            if (context is RadGridView) {
                RadGridView gridView = context as RadGridView;
                gridView.SortDescriptors.Clear();
                gridView.GroupDescriptors.Clear();
                gridView.Columns
                    .OfType<GridViewColumn>()
                    .Where(c => c.ColumnFilterDescriptor.IsActive)
                    .ToList().ForEach(c => c.ClearFilters());
            }
        }

        public object InitializeValue(CustomPropertyInfo customPropertyInfo, object context) {
            return null;
        }

        public object ProvideValue(CustomPropertyInfo customPropertyInfo, object context) {
            RadGridView gridView = context as RadGridView;

            switch (customPropertyInfo.Name) {
                case "Columns": {
                        List<ColumnProxy> columnProxies = new List<ColumnProxy>();

                        foreach (GridViewColumn column in gridView.Columns) {
                            columnProxies.Add(new ColumnProxy() {
                                UniqueName = column.UniqueName,
                                DisplayOrder = column.DisplayIndex,
                                Width = column.Width,
                                IsVisible = column.IsVisible
                            });
                        }

                        return columnProxies;
                    }

                case "SortDescriptors": {
                        List<SortDescriptorProxy> sortDescriptorProxies = new List<SortDescriptorProxy>();

                        foreach (ISortDescriptor descriptor in gridView.SortDescriptors) {
                            if (descriptor is ColumnSortDescriptor == false) {
                                continue;
                            }
                            sortDescriptorProxies.Add(new SortDescriptorProxy() {
                                ColumnUniqueName = ((ColumnSortDescriptor)descriptor).Column.UniqueName,
                                SortDirection = descriptor.SortDirection,
                            });
                        }

                        return sortDescriptorProxies;
                    }

                case "GroupDescriptors": {
                        List<GroupDescriptorProxy> groupDescriptorProxies = new List<GroupDescriptorProxy>();

                        foreach (ColumnGroupDescriptor descriotor in gridView.GroupDescriptors) {
                            groupDescriptorProxies.Add(new GroupDescriptorProxy() {
                                ColumnUniqueName = descriotor.Column.UniqueName,
                                SortDirection = descriotor.SortDirection,
                            });
                        }

                        return groupDescriptorProxies;
                    }

                case "FilterDescriptors": {
                        List<FilterSetting> filterSettings = new List<FilterSetting>();

                        foreach (IFilterDescriptor filterDescriptor in gridView.FilterDescriptors) {
                            if (filterDescriptor is IColumnFilterDescriptor) {
                                var columnFilter = (IColumnFilterDescriptor)filterDescriptor;

                                FilterSetting columnFilterSetting = new FilterSetting();

                                columnFilterSetting.ColumnUniqueName = columnFilter.Column.UniqueName;

                                columnFilterSetting.SelectedDistinctValues.AddRange(columnFilter.DistinctFilter.DistinctValues);

                                if (columnFilter.FieldFilter.Filter1.IsActive) {
                                    columnFilterSetting.Filter1 = new FilterDescriptorProxy();
                                    columnFilterSetting.Filter1.Operator = columnFilter.FieldFilter.Filter1.Operator;
                                    columnFilterSetting.Filter1.Value = columnFilter.FieldFilter.Filter1.Value;
                                    columnFilterSetting.Filter1.IsCaseSensitive = columnFilter.FieldFilter.Filter1.IsCaseSensitive;
                                }

                                columnFilterSetting.FieldFilterLogicalOperator = columnFilter.FieldFilter.LogicalOperator;

                                if (columnFilter.FieldFilter.Filter2.IsActive) {
                                    columnFilterSetting.Filter2 = new FilterDescriptorProxy();
                                    columnFilterSetting.Filter2.Operator = columnFilter.FieldFilter.Filter2.Operator;
                                    columnFilterSetting.Filter2.Value = columnFilter.FieldFilter.Filter2.Value;
                                    columnFilterSetting.Filter2.IsCaseSensitive = columnFilter.FieldFilter.Filter2.IsCaseSensitive;
                                }

                                filterSettings.Add(columnFilterSetting);
                            }
                        }

                        return filterSettings;
                    }
            }

            return null;
        }

        public void RestoreValue(CustomPropertyInfo customPropertyInfo, object context, object value) {
            RadGridView gridView = context as RadGridView;

            switch (customPropertyInfo.Name) {
                case "Columns": {
                        List<ColumnProxy> columnProxies = value as List<ColumnProxy>;

                        foreach (ColumnProxy proxy in columnProxies) {
                            GridViewColumn column = gridView.Columns[proxy.UniqueName];
                            if (column != null) {
                                column.DisplayIndex = proxy.DisplayOrder;
                                column.Width = proxy.Width;
                                column.IsVisible = proxy.IsVisible;
                            }
                        }
                    }
                    break;

                case "SortDescriptors": {
                        // NOTE: Escapes sort patterns
                        if (((FrameworkElement)context).Name != "gridRecords") {
                            gridView.SortDescriptors.SuspendNotifications();

                            gridView.SortDescriptors.Clear();

                            List<SortDescriptorProxy> sortDescriptoProxies = value as List<SortDescriptorProxy>;

                            foreach (SortDescriptorProxy proxy in sortDescriptoProxies) {
                                GridViewColumn column = gridView.Columns[proxy.ColumnUniqueName];
                                gridView.SortDescriptors.Add(new ColumnSortDescriptor() { Column = column, SortDirection = proxy.SortDirection });
                            }

                            gridView.SortDescriptors.ResumeNotifications();
                        }
                    }
                    break;

                case "GroupDescriptors": {
                        gridView.GroupDescriptors.SuspendNotifications();

                        gridView.GroupDescriptors.Clear();

                        List<GroupDescriptorProxy> groupDescriptorProxies = value as List<GroupDescriptorProxy>;

                        foreach (GroupDescriptorProxy proxy in groupDescriptorProxies) {
                            GridViewColumn column = gridView.Columns[proxy.ColumnUniqueName];
                            gridView.GroupDescriptors.Add(new ColumnGroupDescriptor() { Column = column, SortDirection = proxy.SortDirection });
                        }

                        gridView.GroupDescriptors.ResumeNotifications();
                    }
                    break;

                case "FilterDescriptors": {
                        gridView.FilterDescriptors.SuspendNotifications();

                        foreach (var c in gridView.Columns) {
                            if (c.ColumnFilterDescriptor.IsActive) {
                                c.ClearFilters();
                            }
                        }

                        List<FilterSetting> filterSettings = value as List<FilterSetting>;

                        foreach (FilterSetting setting in filterSettings) {
                            var column = gridView.Columns[setting.ColumnUniqueName];

                            IColumnFilterDescriptor columnFilter = column.ColumnFilterDescriptor;

                            foreach (object distinctValue in setting.SelectedDistinctValues) {
                                columnFilter.DistinctFilter.AddDistinctValue(distinctValue);
                            }

                            if (setting.Filter1 != null) {
                                columnFilter.FieldFilter.Filter1.Operator = setting.Filter1.Operator;
                                columnFilter.FieldFilter.Filter1.Value = setting.Filter1.Value;
                                columnFilter.FieldFilter.Filter1.IsCaseSensitive = setting.Filter1.IsCaseSensitive;
                            }

                            columnFilter.FieldFilter.LogicalOperator = setting.FieldFilterLogicalOperator;

                            if (setting.Filter2 != null) {
                                columnFilter.FieldFilter.Filter2.Operator = setting.Filter2.Operator;
                                columnFilter.FieldFilter.Filter2.Value = setting.Filter2.Value;
                                columnFilter.FieldFilter.Filter2.IsCaseSensitive = setting.Filter2.IsCaseSensitive;
                            }
                        }

                        gridView.FilterDescriptors.ResumeNotifications();
                    }
                    break;
            }
        }
    }
}
