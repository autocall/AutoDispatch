using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoCall.Logic.ViewModels;
using AutoCall.WPF.Extensions.Core;

namespace AutoCall.WPF.Extensions {
    /// <summary>
    ///     Represents extension methods for DataGrid control </summary>
    public static class DataGridExtensions {
        #region [ Private Methods ]

        /// <summary>
        ///     Creates images column </summary>
        /// <param name="header">Column header text</param>
        /// <param name="images">Images info</param>
        /// <returns>Images column</returns>
        private static DataGridTemplateColumn CreateImagesColumn(string header, List<DataGridImageColumnInfo> images) {
            var gridFactory = new FrameworkElementFactory(typeof (Grid));

            var template = new DataTemplate();
            template.VisualTree = gridFactory;

            for (int i = 0; i < images.Count; i++) {
                DataGridImageColumnInfo info = images[i];

                var columnDefinitionFactory = new FrameworkElementFactory(typeof (ColumnDefinition));
                columnDefinitionFactory.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
                gridFactory.AppendChild(columnDefinitionFactory);

                var imageFactory = new FrameworkElementFactory(typeof (Image));
                imageFactory.Name = info.ControlName;
                imageFactory.SetValue(Image.WidthProperty, 20d);
                imageFactory.SetValue(Image.HeightProperty, 20d);
                imageFactory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                imageFactory.SetValue(Image.SourceProperty, info.Image);
                imageFactory.SetValue(Grid.ColumnProperty, i);
                gridFactory.AppendChild(imageFactory);

                var trigger = new DataTrigger {
                    Value = false,
                    Binding = new Binding(info.Property),
                    Setters = {
                        new Setter {
                            Property = Image.VisibilityProperty,
                            Value = Visibility.Hidden,
                            TargetName = info.ControlName
                        }
                    }
                };

                template.Triggers.Add(trigger);
            }
            var column = new DataGridTemplateColumn();
            column.Header = header;
            column.CellTemplate = template;

            return column;
        }

        /// <summary>
        ///     Creates image column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="imgControlName">Name of image control</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="img">Image</param>
        /// <returns>Image column</returns>
        private static DataGridTemplateColumn CreateImageColumn(string header, string imgControlName, string bindingProperty,
            BitmapImage img) {
            var factory = new FrameworkElementFactory(typeof (Image));
            factory.Name = imgControlName;
            factory.SetValue(Image.WidthProperty, 20d);
            factory.SetValue(Image.HeightProperty, 20d);
            factory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
            factory.SetValue(Image.SourceProperty, img);

            var trigger = new DataTrigger {
                Value = false,
                Binding = new Binding(bindingProperty) {
                },
                Setters = {
                    new Setter {
                        Property = Image.VisibilityProperty,
                        Value = Visibility.Hidden,
                        TargetName = imgControlName
                    }
                }
            };

            var template = new DataTemplate();
            template.VisualTree = factory;
            template.Triggers.Add(trigger);

            var column = new DataGridTemplateColumn();
            column.Header = header;
            column.CellTemplate = template;

            return column;
        }

        /// <summary>
        ///     Creates simple text column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <returns>Text column</returns>
        private static DataGridTextColumn CreateTextColumn(string header, string bindingProperty) {
            return new DataGridTextColumn() {
                Header = header,
                Binding = new Binding(bindingProperty),
            };
        }

        /// <summary>
        ///     Creates simple hyperlink column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <returns>Text column</returns>
        private static DataGridHyperlinkColumn CreateHyperlinkColumn(string header, string bindingProperty) {
            return new DataGridHyperlinkColumn() {
                Header = header,
                Binding = new Binding(bindingProperty),
            };
        }

        /// <summary>
        ///     Creates simple text column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <returns>Text column</returns>
        private static DataGridTextColumn CreateTextColumn(string header, BindingBase binding) {
            return new DataGridTextColumn() {
                Header = header,
                Binding = binding,
            };
        }

        /// <summary>
        ///     Creates simple hyperlink column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <returns>Text column</returns>
        private static DataGridHyperlinkColumn CreateHyperlinkColumn(string header, BindingBase binding) {
            return new DataGridHyperlinkColumn() {
                Header = header,
                Binding = binding,
            };
        }

        /// <summary>
        ///     Gets visual child </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++) {
                var v = (Visual) VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null) {
                    child = GetVisualChild<T>(v);
                }
                if (child != null) {
                    break;
                }
            }
            return child;
        }

        #endregion [ Private Methods ]

        #region [ Public Methods ]

        /// <summary>
        ///     Creates images column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="images">Images info</param>
        public static void CreateImagesColumn(this DataGrid grid, string header, List<DataGridImageColumnInfo> images) {
            DataGridTemplateColumn column = CreateImagesColumn(header, images);
            column.CanUserReorder = false;
            column.CanUserResize = false;
            column.CanUserSort = false;
            grid.Columns.Add(column);
        }

        /// <summary>
        ///     Creates image column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="imgControlName">Name of image control</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="img">Image</param>
        public static void CreateImageColumn(this DataGrid grid, string header, string imgControlName, string bindingProperty,
            BitmapImage img) {
            DataGridTemplateColumn column = CreateImageColumn(header, imgControlName, bindingProperty, img);
            grid.Columns.Add(column);
        }

        /// <summary>
        ///     Creates hyperlink column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="settings">Column settings</param>
        /// <param name="converter">Value converter</param>
        /// <param name="widthConverter">Width converter</param>
        /// <param name="visibilityConverter">Visibility converter</param>
        /// <param name="format">String format</param>
        public static void DataGridHyperlinkColumn(this DataGrid grid, string header, string bindingProperty, string sortMemberPath,
            GridColumnSettingsViewModel settings, IValueConverter converter, IValueConverter widthConverter,
            IValueConverter visibilityConverter, string format = null) {
            DataGridHyperlinkColumn column = CreateHyperlinkColumn(header, bindingProperty);
            column.SortMemberPath = sortMemberPath;
            if (!String.IsNullOrEmpty(format))
                column.Binding.StringFormat = format;
            if (converter != null)
                ((Binding)column.Binding).Converter = converter;
            if (settings != null) {
                BindingOperations.SetBinding(column, DataGridColumn.DisplayIndexProperty,
                    new Binding("Index") { Source = settings, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.WidthProperty,
                    new Binding("Width") { Source = settings, Converter = widthConverter, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.VisibilityProperty,
                    new Binding("Enabled") { Source = settings, Converter = visibilityConverter, Mode = BindingMode.TwoWay });
            }
            grid.Columns.Add(column);
        }

        /// <summary>
        ///     Creates text column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="settings">Column settings</param>
        /// <param name="converter">Value converter</param>
        /// <param name="widthConverter">Width converter</param>
        /// <param name="visibilityConverter">Visibility converter</param>
        /// <param name="format">String format</param>
        public static void CreateTextColumn(this DataGrid grid, string header, string bindingProperty, string sortMemberPath,
            GridColumnSettingsViewModel settings, IValueConverter converter, IValueConverter widthConverter,
            IValueConverter visibilityConverter, string format = null) {
            DataGridTextColumn column = CreateTextColumn(header, bindingProperty);
            column.SortMemberPath = sortMemberPath;
            if (!String.IsNullOrEmpty(format))
                column.Binding.StringFormat = format;
            if (converter != null)
                ((Binding)column.Binding).Converter = converter;
            if (settings != null) {
                BindingOperations.SetBinding(column, DataGridColumn.DisplayIndexProperty,
                    new Binding("Index") { Source = settings, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.WidthProperty,
                    new Binding("Width") { Source = settings, Converter = widthConverter, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.VisibilityProperty,
                    new Binding("Enabled") { Source = settings, Converter = visibilityConverter, Mode = BindingMode.TwoWay });
            }
            grid.Columns.Add(column);
        }

        /// <summary>
        ///     Creates text column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="settings">Column settings</param>
        /// <param name="widthConverter">Width converter</param>
        /// <param name="visibilityConverter">Visibility converter</param>
        /// <param name="format">String format</param>
        public static void CreateTextColumn(this DataGrid grid,
            string header, 
            BindingBase binding,
            string sortMemberPath,
            GridColumnSettingsViewModel settings, 
            IValueConverter widthConverter,
            IValueConverter visibilityConverter,
            string format = null) {
            DataGridTextColumn column = CreateTextColumn(header, binding);
            column.SortMemberPath = sortMemberPath;
            if (settings != null) {
                BindingOperations.SetBinding(column, DataGridColumn.DisplayIndexProperty,
                    new Binding("Index") { Source = settings, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.WidthProperty,
                    new Binding("Width") { Source = settings, Converter = widthConverter, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(column, DataGridColumn.VisibilityProperty,
                    new Binding("Enabled") { Source = settings, Converter = visibilityConverter, Mode = BindingMode.TwoWay });
            }
            grid.Columns.Add(column);
        }

        /// <summary>
        ///     Gets datagrid cell </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="row">Datagrid row</param>
        /// <param name="column">Column index</param>
        /// <returns>Datagrid cell</returns>
        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column) {
            if (row != null) {
                var presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null) {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                var cell = (DataGridCell) presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        /// <summary>
        ///     Select datagrid row by index </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="rowIndex">Datagrid row index</param>
        public static void SelectRowByIndex(this DataGrid grid, int rowIndex) {
            var row = grid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            if (row != null) {
                DataGridCell cell = GetCell(grid, row, 0);
                if (cell != null)
                    cell.Focus();
            }
        }

        #endregion [ Public Methods ]
    }
}