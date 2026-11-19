using System;
using AutoDispatch.Logic.Converters;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Settings;
using AutoCall.Logic.Settings.Sections;
using AutoDispatch.Logic.Contracts.Enums;
using AutoDispatch.Logic.Helpers;
using AutoDispatch.Logic.Contracts.ViewModels.Pages;
using AutoMapper;
using Telerik.Windows.Controls;
using AutoCall.WPF.Extensions;
using System.Diagnostics;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class FiltersDialog : BaseDispatchDialog {

        protected override RadProgressBar ProgressBar => progressBar;
        protected override TextBlock ErrorTextBlock => tbError;
        protected override Button OkButton => btnOk;
        protected override Button CloseButton => btnClose;

        private SettingsService _service;
        private IMapper _mapper;

        /// <summary>
        ///     Gets or sets dialog view model </summary>
        public FiltersColumnsViewModel Model {
            get { return DataContext as FiltersColumnsViewModel; }
            set { DataContext = value; }
        }

        #region [ .ctor ]

        /// <summary>
        ///     Constructor </summary>
        public FiltersDialog(SettingsService service, IMapper mapper, FiltersColumnsViewModel filters) {
            _service = service;
            _mapper = mapper;
            Model = filters;

            InitializeComponent();

            Title = $"{filters.Table.GetDescription()} - Filters";
        }

        #endregion [ .ctor ]

        #region [ Add/Edit/Delete Event Handlers ]

        private void btnAddColumn_OnClick(object sender, RoutedEventArgs args) {
            var vm = (FilterColumnsViewModel)((FrameworkElement)sender).DataContext;
            if (vm.NewKey == null || vm.NewValue == null) {
                MessageBoxEx.Error(this, "Please enter column name and value");
                return;
            }
            if (ValidDateSpan(vm.NewKey.Value, vm.NewValue) == false) {
                MessageBoxEx.Error(this, "Please enter a valid value");
                return;
            }
            if (ValidLocation(vm.NewKey.Value, vm.NewValue) == false) {
                MessageBoxEx.Error(this, "Please enter a valid Location\n Chicago - city (first capital)\n ILOH - states (all capitals)\n Z1Z2 - zones");
                return;
            }

            if (vm.Columns.Any(e => e.Column == vm.NewKey.Value)) {
                vm.Columns.Remove(vm.Columns.First(e => e.Column == vm.NewKey.Value));
            }

            vm.Columns.Add(new FilterColumnViewModel(vm.NewKey.Value, vm.NewValue));

            vm.NewKey = null;
            vm.NewValue = null;
        }

        private bool ValidDateSpan(FilterColumnEnum newKey, string newValue) {
            if (newKey == FilterColumnEnum.Pickup || newKey == FilterColumnEnum.Delivery) {
                return Enum.GetNames(typeof(DayOfWeek)).Contains(newValue.FirstCharToUpper()) ||
                    Regex.IsMatch(newValue, FilterHelper.DateSpanRegex) ||
                    Regex.IsMatch(newValue, FilterHelper.DateSpanFromRegex) ||
                    Regex.IsMatch(newValue, FilterHelper.DateSpanToRegex) ||
                    Regex.IsMatch(newValue, FilterHelper.DateSpanBetweenRegex);
            }
            return true;
        }

        private bool ValidLocation(FilterColumnEnum newKey, string newValue) {
            if (newKey == FilterColumnEnum.Origin || newKey == FilterColumnEnum.Destination) {
                if (newValue.All(e => Char.IsLower(e))) {
                    return false;
                }
            }
            return true;
        }

        private void btnAddFilter_OnClick(object sender, RoutedEventArgs e) {
            Model.Filters.Add(new FilterColumnsViewModel());
        }

        private void imgDeleteColumn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            var vm = (FilterColumnViewModel)((FrameworkElement)sender).DataContext;

            if (MessageBox.Show($"Please confirm to delete column '{vm.Column}: {vm.Value}'", "Delete column", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) {
                return;
            }

            foreach (var filter in Model.Filters) {
                foreach (var column in filter.Columns) {
                    if (column == vm) {
                        filter.Columns.Remove(vm);
                        return;
                    }
                }
            }
        }

        private void btnDeleteFilter_OnClick(object sender, RoutedEventArgs e) {
            var vm = (FilterColumnsViewModel)((FrameworkElement)sender).DataContext;

            if (MessageBox.Show($"Please confirm to delete filter '{vm.Name}'", "Delete filter", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel) {
                return;
            }

            Model.Filters.Remove(vm);
        }

        #endregion [ Add/Edit/Delete Event Handlers ]

        #region [ Ok/Cancel Event Handlers ]

        private async void btnOk_OnClick(object sender, RoutedEventArgs e) {
            // TO TRIM STRINGS
            foreach (var filter in Model.Filters) {
                if (String.IsNullOrEmpty(filter.Name)) {
                    MessageBoxEx.Error(this, "Please enter 'name'");
                    return;
                }
                if (String.IsNullOrEmpty(filter.NewValue) == false) {
                    MessageBoxEx.Error(this, "Please press '+' or clear 'value'");
                    return;
                }
                filter.Name = filter.Name.Trim();
                foreach (var column in filter.Columns) {
                    column.Value = column.Value.Trim();
                }
            }
            var settings = FiltersColumnsConverter.ToSettings(Model);
            _service.SetFiltersColumns((byte)Model.Table, settings);
            base.BusyStart();
            await Task.Factory.StartNew(() => _service.SaveSettings());
            DialogResult = true;
        }

        private void btnClose_OnClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Do you want to save changes?", "Saving", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) == MessageBoxResult.Yes) {
                e.Handled = true;
                btnOk_OnClick(sender, e);
            } else {
                DialogResult = false;
            }
        }

        #endregion [ Ok/Cancel Event Handlers ]

        private void CustomStatusLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
        }
    }
}