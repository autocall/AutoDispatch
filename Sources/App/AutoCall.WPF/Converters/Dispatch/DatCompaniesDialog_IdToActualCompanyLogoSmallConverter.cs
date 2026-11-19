using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using AutoDispatch.Logic.ViewModels.Pages;

namespace AutoCall.WPF.Converters.Dispatch {
    public class DatCompaniesDialog_IdToActualCompanyLogoSmallConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return DispatchBoardViewModel.Instance.ActualCompanies.FirstOrDefault(e => e.EntityId == (Guid?)value)?.LogoSmall;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
