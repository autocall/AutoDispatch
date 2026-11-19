using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.ViewModels.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace AutoCall.WPF.UserControls {
    public class SumDataColumn : GridViewDataColumn {

        private RadGridView gridView => (RadGridView)this.DataControl;
        private TextBlock footer => (TextBlock)this.Footer;

        public string SumMemberPath { get; set; }

        public SumDataColumn() {
            IsReadOnly = true;
        }

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            //if (this.DataMemberBinding?.StringFormat != null) {
            //    this.AggregateFunctions.Add(new SumFunction() { ResultFormatString = $"{{0:{this.DataMemberBinding.StringFormat}}}" });
            //} else {
            //    this.AggregateFunctions.Add(new SumFunction());
            //}
            Footer = new TextBlock();

            gridView.SelectionChanged += CustomGridViewDataColumn_SelectionChanged;
            gridView.DataLoaded += GridView_DataLoaded;
        }

        private void GridView_DataLoaded(object sender, EventArgs e) {
            Calculate();
        }

        private void CustomGridViewDataColumn_SelectionChanged(object sender, SelectionChangeEventArgs args) {
            Calculate();
        }

        private Task task = null;
        private void Calculate() {
            Task.Factory.StartNew(async () => {
                #region [ single request + delay ]
                var localTask = task = Task.Delay(300);
                await task;
                if (localTask != task) {
                    return;
                }
                #endregion [ single request + delay ]

                var source = await Dispatcher.InvokeAsync((Func<IEnumerable>)(() => {
                    if (gridView == null) return null;
                    return (gridView.SelectedItems.Any() ? (IEnumerable)gridView.SelectedItems : (ICollectionView)gridView.ItemsSource)?.Cast<object>().ToList();
                }));

                dynamic tmpSum = null;
                if (source == null) return;
                foreach (var wrapper in source) {
                    dynamic obj = ReflectionHelper.GetPropValue(wrapper, this.SumMemberPath ?? this.DataMemberBinding.Path.Path);
                    if (obj is string) {
                        break;
                    }
                    if (obj != null) {
                        if (tmpSum == null) {
                            tmpSum = obj;
                        } else {
                            tmpSum = tmpSum + obj;
                        }
                    }
                }
                await Dispatcher.InvokeAsync((Action)(() => {
                    var run = new Run() { Text = tmpSum != null ? tmpSum.ToString(this.DataMemberBinding.StringFormat) : null };
                    if (gridView.SelectedItems.Any()) {
                        run.FontStyle = FontStyles.Italic;
                    }
                    footer.Inlines.Clear();
                    footer.Inlines.Add(run);
                }));
            });
        }
    }
}
