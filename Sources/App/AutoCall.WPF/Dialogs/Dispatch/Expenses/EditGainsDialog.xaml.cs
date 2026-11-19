using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditGainsDialog : Window {
        private decimal? LineHaul { get; }

        public List<GainViewModel> ViewModel {
            get { return DataContext as List<GainViewModel>; }
            set { DataContext = value; }
        }

        public EditGainsDialog(decimal? lineHaul) {
            LineHaul = lineHaul;
            InitializeComponent();
        }

        private void tbGain_OnLoaded(object sender, RoutedEventArgs e) {
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Delete_OnClicked(object sender, RoutedEventArgs e) {
            var gain = (GainViewModel)((FrameworkElement)sender).DataContext;
            if (MessageBox.Show("Please confirm to delete gain\n" + gain.ToString(), "Delete gain", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }
            gain.Cost = null;
            gain.Comment = null;
            gain.Paid = false;
            gain.PaidDate = null;
        }

        private void Calculate_OnClicked(object sender, RoutedEventArgs e) {
            var gain = (GainViewModel)((FrameworkElement)sender).DataContext;
            if (gain.IsCalculable == false) {
                MessageBox.Show("This gain is not calculable");
                return;
            }
            switch (gain.Type) {
                case GainType.LRForService12:
                    gain.Cost = -(LineHaul + this.ViewModel.Where(x => x.IsCalculable == false && x.Cost != null).Sum(x => x.Cost)) * 0.12m;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Paid_OnChecked(object sender, RoutedEventArgs e) {
            ((GainViewModel)((FrameworkElement)sender).DataContext).PaidDate = DateTime.Now;
        }

        private void Paid_Unchecked(object sender, RoutedEventArgs e) {
            ((GainViewModel)((FrameworkElement)sender).DataContext).PaidDate = null;
        }
    }
}
