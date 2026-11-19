using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using System.Windows.Media;
using AutoDispatch.Logic.ViewModels.Items;
using System.Linq;
using System;

namespace AutoCall.WPF.UserControls {
    public class RowNumberColumn : GridViewDataColumn {
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof(int), typeof(RowNumberColumn), new UIPropertyMetadata(0));
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize", typeof(int), typeof(RowNumberColumn), new UIPropertyMetadata(0));

        public int PageIndex {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }
        public int PageSize {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }
        public RowNumberColumn() {
            this.AggregateFunctions.Add(new CountFunction());
            this.CellStyle = new Style(typeof(GridViewCell)) {
                Setters = {
                    new Setter(Control.ForegroundProperty, Brushes.DarkGray),
                    new Setter(Control.FontSizeProperty, 10.0),
                }
            };
            this.IsReadOnly = true;
            this.IsSortable = false;
            this.IsResizable = false;
        }

        public override FrameworkElement CreateCellElement(GridViewCell cell, object dataItem) {
            TextBlock textBlock = cell.Content as TextBlock;

            if (textBlock == null) {
                textBlock = new TextBlock();
            }
            try {
                textBlock.Text = (PageSize * PageIndex + this.DataControl.Items.IndexOf(dataItem) + 1).ToString();
            } catch (Exception e) {
            }
            return textBlock;
        }
    }
}
