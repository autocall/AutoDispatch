using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoCall.Common;
using AutoCall.Common.Helpers;
using AutoCall.WPF.Converters;
using AutoCall.WPF.Dialogs.Dispatch.UserControls;
using AutoCall.WPF.Extensions.Core;
using AutoCall.WPF.UserControls;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Persistence;
using Telerik.Windows.Persistence.Services;
using GridViewColumn = Telerik.Windows.Controls.GridViewColumn;
using RadMenuItem = Telerik.Windows.Controls.RadMenuItem;

namespace AutoCall.WPF.Extensions {
    /// <summary>
    ///     Represents extension methods for GridView control </summary>
    public static class GridViewExtensions {

        #region [ Private Methods ]

        /// <summary>
        ///     Creates images column </summary>
        /// <param name="header">Column header text</param>
        /// <param name="images">Images info</param>
        /// <returns>Images column</returns>
        private static GridViewDataColumn CreateImagesColumn(object header, List<DataGridImageColumnInfo> images) {
            var gridFactory = new FrameworkElementFactory(typeof(Grid));

            var template = new DataTemplate();
            template.VisualTree = gridFactory;

            for (int i = 0; i < images.Count; i++) {
                DataGridImageColumnInfo info = images[i];

                var columnDefinitionFactory = new FrameworkElementFactory(typeof(ColumnDefinition));
                columnDefinitionFactory.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Auto));
                gridFactory.AppendChild(columnDefinitionFactory);

                var imageFactory = new FrameworkElementFactory(typeof(Image));
                imageFactory.Name = info.ControlName;
                imageFactory.SetValue(Image.WidthProperty, 20d);
                imageFactory.SetValue(Image.HeightProperty, 20d);
                imageFactory.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
                imageFactory.SetValue(Image.SourceProperty, info.Image);
                imageFactory.SetValue(Grid.ColumnProperty, i);
                imageFactory.SetValue(Image.VisibilityProperty,
                    new Binding(info.Property) {
                        Mode = BindingMode.OneWay,
                        Converter = new Telerik.Windows.Controls.BooleanToVisibilityConverter()
                    });
                gridFactory.AppendChild(imageFactory);
                // NOTE: http://www.telerik.com/forums/datatemplate-and-datatrigger-in-grid
                // What I have found is that any triggers in the datatemplate will be ignored due to the way RadGrirdView loads the CellTemplate
                //var trigger = new DataTrigger {
            }
            template.Seal();

            var column = new GridViewDataColumn();
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
        private static GridViewImageColumn CreateImageColumn(string header, string imgControlName, string bindingProperty,
            BitmapImage img) {
            var factory = new FrameworkElementFactory(typeof(Image));
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

            var column = new GridViewImageColumn();
            column.Header = header;
            column.CellTemplate = template;

            return column;
        }

        /// <summary>
        ///     Creates simple text column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <returns>Text column</returns>
        private static GridViewDataColumn CreateTextColumn(string header, string bindingProperty) {
            return new GridViewDataColumn() {
                Header = header,
                DataMemberBinding = new Binding(bindingProperty) { Mode = BindingMode.OneWay },
            };
        }

        /// <summary>
        ///     Creates simple text column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <returns>Text column</returns>
        private static SumDataColumn CreateSumColumn(string header, string bindingProperty) {
            return new SumDataColumn() {
                Header = header,
                DataMemberBinding = new Binding(bindingProperty) { Mode = BindingMode.OneWay },
            };
        }

        /// <summary>
        ///     Creates simple hyperlink column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <returns>Text column</returns>
        private static GridViewHyperlinkColumn CreateHyperlinkColumn(string header, string bindingProperty) {
            return new GridViewHyperlinkColumn() {
                Header = header,
                DataMemberBinding = new Binding(bindingProperty),
            };
        }

        /// <summary>
        ///     Creates simple text column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <returns>Text column</returns>
        private static GridViewDataColumn CreateTextColumn(string header, Binding binding) {
            return new GridViewDataColumn() {
                Header = header,
                DataMemberBinding = binding,
            };
        }

        /// <summary>
        ///     Creates simple hyperlink column with header and binding properties </summary>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <returns>Text column</returns>
        private static GridViewHyperlinkColumn CreateHyperlinkColumn(string header, Binding binding) {
            return new GridViewHyperlinkColumn() {
                Header = header,
                DataMemberBinding = binding,
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
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
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
        ///     Creates image column with header and binding properties </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property for ImageSource</param>
        /// <returns>Image column</returns>
        public static GridViewImageColumn CreateImageColumn(this RadGridView grid, string header, string bindingProperty) {
            var column = new GridViewImageColumn();
            column.Header = header;
            column.DataMemberBinding = new Binding(bindingProperty) { Mode = BindingMode.OneWay };
            column.ImageWidth = 20d;
            column.ImageHeight = 20d;
            column.IsGroupable = false;
            column.IsSortable = false;
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates images column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="images">Images info</param>
        public static GridViewDataColumn CreateImagesColumn(this RadGridView grid, object header, List<DataGridImageColumnInfo> images) {
            var column = CreateImagesColumn(header, images);
            column.IsReorderable = false;
            column.IsResizable = false;
            column.IsSortable = false;
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates image column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="imgControlName">Name of image control</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="img">Image</param>
        public static GridViewImageColumn CreateImageColumn(this RadGridView grid, string header, string imgControlName, string bindingProperty,
            BitmapImage img) {
            GridViewImageColumn column = CreateImageColumn(header, imgControlName, bindingProperty, img);
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates hyperlink column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="converter">Value converter</param>
        /// <param name="format">String format</param>
        public static GridViewColumn GridViewHyperlinkColumn(this RadGridView grid,
            string header,
            string bindingProperty,
            IValueConverter converter = null,
            string sortMemberPath = null,
           string format = null) {
            GridViewHyperlinkColumn column = CreateHyperlinkColumn(header, bindingProperty);
            column.SortMemberPath = sortMemberPath;
            if (!String.IsNullOrEmpty(format))
                column.DataMemberBinding.StringFormat = format;
            if (converter != null)
                ((Binding)column.DataMemberBinding).Converter = converter;
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates hyperlink column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        public static GridViewPhoneColumn GridViewPhoneColumn(this RadGridView grid,
            string header,
            string bindingProperty) {
            GridViewPhoneColumn column = new GridViewPhoneColumn { Header = header };
            column.DataMemberBinding = new Binding(bindingProperty) { Path = new PropertyPath(bindingProperty), Mode = BindingMode.OneWay };
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates text column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="converter">Value converter</param>
        /// <param name="format">String format</param>
        public static GridViewColumn CreateTextColumn(this RadGridView grid,
            string header,
            string bindingProperty,
            IValueConverter converter = null,
            string sortMemberPath = null,
            string format = null) {
            GridViewDataColumn column = CreateTextColumn(header, bindingProperty);
            column.SortMemberPath = sortMemberPath;
            if (!String.IsNullOrEmpty(format))
                column.DataMemberBinding.StringFormat = format;
            if (converter != null)
                ((Binding)column.DataMemberBinding).Converter = converter;

            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates text column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="binding">Binding object</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="format">String format</param>
        public static GridViewColumn CreateTextColumn(this RadGridView grid,
            string header,
            Binding binding,
            string sortMemberPath,
            string format = null) {
            GridViewDataColumn column = CreateTextColumn(header, binding);
            column.SortMemberPath = sortMemberPath;
            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates sum column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="converter">Value converter</param>
        /// <param name="format">String format</param>
        public static GridViewColumn CreateSumColumn(this RadGridView grid,
            string header,
            string bindingProperty,
            IValueConverter converter = null,
            string sortMemberPath = null,
            string format = null) {
            GridViewDataColumn column = CreateSumColumn(header, bindingProperty);
            column.SortMemberPath = sortMemberPath;
            if (!String.IsNullOrEmpty(format))
                column.DataMemberBinding.StringFormat = format;
            if (converter != null)
                ((Binding)column.DataMemberBinding).Converter = converter;

            grid.Columns.Add(column);
            return column;
        }

        /// <summary>
        ///     Creates template column </summary>
        /// <param name="grid">Source grid</param>
        /// <param name="header">Column header text</param>
        /// <param name="bindingProperty">Binding property</param>
        /// <param name="sortMemberPath">Sort member path</param>
        /// <param name="converter">Value converter</param>
        /// <param name="format">String format</param>
        public static GridViewColumn CreateTemplateColumn(this RadGridView grid,
            string header,
            string bindingProperty,
            DataTemplate template) {
            GridViewDataColumn column = new GridViewDataColumn();
            column.Header = header;
            column.UniqueName = bindingProperty;
            column.CellTemplate = template;
            grid.Columns.Add(column);
            return column;
        }

        public static void SetColumnStyle1(this GridViewColumn column, string bindingBoolProperty, string bindingParentProperty) {
            column.CellStyle.Triggers.Add(new DataTrigger() {
                Value = true,
                Binding = new Binding(bindingBoolProperty) { Mode = BindingMode.OneWay },
                Setters = { new Setter(GridViewCell.BackgroundProperty, Brushes.LightGray) }
            });
            column.CellStyle.Triggers.Add(new DataTrigger() {
                Value = true,
                Binding = new Binding(bindingParentProperty) { Mode = BindingMode.OneWay, Converter = new NotNullToBoolConverter() },
                Setters = { new Setter(GridViewCell.ForegroundProperty, Brushes.Gray) }
            });
        }

        public static void SetColumnStyle2(this GridViewColumn column, string bindingParentProperty) {
            column.CellStyle = new Style(typeof(GridViewCell), (Style)column.DataControl.Resources[typeof(GridViewCell)]);
            column.CellStyle.Triggers.Add(new DataTrigger() {
                Value = true,
                Binding = new Binding(bindingParentProperty) { Mode = BindingMode.OneWay, Converter = new NotNullToBoolConverter() },
                Setters = { new Setter(GridViewCell.ForegroundProperty, Brushes.Gray) }
            });
        }

        public static RadRoutedEventHandler RadContextMenu_OnOpening => radContextMenu_OnOpening;
        private static void radContextMenu_OnOpening(object sender, RadRoutedEventArgs e) {
            var menu = (RadContextMenu)sender;
            var row = menu.GetClickedElement<GridViewHeaderRow>();
            var cell = menu.GetClickedElement<GridViewHeaderCell>();
            if (row == null && cell == null) {
                menu.IsOpen = false;
            }
        }

        private static GridViewHeaderCell clickedElement;
        public static RadRoutedEventHandler RadMenuItem_Click => radMenuItem_Click;
        private static void radMenuItem_Click(object sender, RadRoutedEventArgs e) {
            var menuColumn = (GridViewColumn)((FrameworkElement)sender).DataContext;
            var tmpClickedElement = ((RadContextMenu)((RadMenuItem)sender).Menu).GetClickedElement<GridViewHeaderCell>();
            if (tmpClickedElement != null) {
                clickedElement = tmpClickedElement;
            }
            var headerColumn = clickedElement?.Column;
            if (headerColumn != null) {
                App.Current.Dispatcher.InvokeAsync(() => {
                    Console.WriteLine($"DisplayIndex {menuColumn.DisplayIndex}->{headerColumn.DisplayIndex + 1}");
                    menuColumn.DisplayIndex = headerColumn.DisplayIndex + 1;
                });
            } else {
                Console.WriteLine($"DisplayIndex ??");
            }
        }

        public static void SetSettings(this RadGridView grid, byte[] data) {
            ServiceProvider.RegisterPersistenceProvider<ICustomPropertyProvider>(typeof(RadGridView), new GridViewCustomPropertyProvider());
            PersistenceManager manager1 = new PersistenceManager();
            try {
                var binary = new MemoryStream(data);
                manager1.Load(grid, binary);
                // fixs Display Indexes
                var index = 0;
                foreach (var column in grid.Columns.Cast<GridViewColumn>().OrderBy(c => c.DisplayIndex)) {
                    if (column.DisplayIndex != index) {
                        column.DisplayIndex = index;
                    }
                    index++;
                }
            } catch (Exception) {
                _l.w("Settings was not loaded");
                throw;
            }
        }

        public static byte[] GetSettings(this RadGridView grid) {
            ServiceProvider.RegisterPersistenceProvider<ICustomPropertyProvider>(typeof(RadGridView), new GridViewCustomPropertyProvider());
            PersistenceManager manager1 = new PersistenceManager();
            var stream1 = manager1.Save(grid);
            stream1.Position = 0;
            return BinaryHelper.ReadFully(stream1);
        }

        #endregion [ Public Methods ]

        public static RoutedEventHandler GridViewCell_Loaded => _gridViewCell_Loaded;
        private static void _gridViewCell_Loaded(object sender, RoutedEventArgs args) {
        }
    }
}