using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoDispatch.Logic.Contracts.ViewModels.Items.Expenses;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public partial class EditExpensesDialog : Window {
        private decimal? LineHaul { get; }

        public List<ExpenseViewModel> ViewModel {
            get { return DataContext as List<ExpenseViewModel>; }
            set { DataContext = value; }
        }

        public EditExpensesDialog(decimal? lineHaul) {
            LineHaul = lineHaul;
            InitializeComponent();
        }

        private void tbExpense_OnLoaded(object sender, RoutedEventArgs e) {
        }

        private void btnOk_Clicked(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Delete_OnClicked(object sender, RoutedEventArgs e) {
            var expense = (ExpenseViewModel)((FrameworkElement)sender).DataContext;
            if (MessageBox.Show("Please confirm to delete expense\n" + expense.ToString(), "Delete expense", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
                return;
            }
            expense.Cost = null;
            expense.Comment = null;
            expense.Paid = false;
            expense.PaidDate = null;
        }

        private void Calculate_OnClicked(object sender, RoutedEventArgs e) {
            var exp = (ExpenseViewModel)((FrameworkElement)sender).DataContext;
            if (exp.IsCalculable == false) {
                MessageBox.Show("This expense is not calculable");
                return;
            }
            switch (exp.Type) {
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Paid_OnChecked(object sender, RoutedEventArgs e) {
            ((ExpenseViewModel)((FrameworkElement)sender).DataContext).PaidDate = DateTime.Now;
        }

        private void Paid_Unchecked(object sender, RoutedEventArgs e) {
            ((ExpenseViewModel)((FrameworkElement)sender).DataContext).PaidDate = null;
        }
    }
}
