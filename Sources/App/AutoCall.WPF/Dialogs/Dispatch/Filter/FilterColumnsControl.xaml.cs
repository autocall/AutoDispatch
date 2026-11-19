using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoCall.WPF.Dialogs.Dispatch.UserControls {
    public partial class FilterColumnsControl : ItemsControl {

        /// <summary>
        ///    Preloaded filter name from settings. </summary>
        /// <remarks> 1. Creates Boards and loads settings (and selected filters). 2. Initializes filters (and list filters). </remarks>
        public string SelectedFilterName { get; set; }

        /// <summary>
        ///     Gets or sets board view model </summary>
        public FiltersColumnsViewModel Model {
            get { return (FiltersColumnsViewModel)DataContext; }
            set { DataContext = value as FiltersColumnsViewModel; }
        }

        public Action SelectionChanged { get; set; }

        public FilterColumnsControl() {
            InitializeComponent();
        }

        private void bFilter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var selection = (FilterColumnsViewModel)((FrameworkElement)sender).DataContext;
            if (Model.SelectedFilter == selection) {
                Model.SelectedFilter = null;
            } else {
                Model.SelectedFilter = selection;
            }
            SelectionChanged();
        }

        private void ItemsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs args) {
            if (SelectedFilterName != null && Model != null) {
                Model.SelectedFilter = Model.Filters.FirstOrDefault(e => e.Name == SelectedFilterName);
            }
        }
    }
}
