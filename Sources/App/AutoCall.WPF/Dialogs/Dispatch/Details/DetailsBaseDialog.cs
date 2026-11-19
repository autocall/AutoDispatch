using System.Linq;
using System.Windows;
using System.Windows.Documents;
using AutoCall.Common.Extensions;
using AutoCall.Logic.Services;
using AutoDispatch.Logic.Services;

namespace AutoCall.WPF.Dialogs.Dispatch {
    public abstract class DetailsBaseDialog : BaseDispatchDialog {
        
        protected DispatchService _dispatchService { get; }
        protected ICallService _callService { get; }

        /// <summary>
        ///     .ctor </summary>
        protected DetailsBaseDialog(
            DispatchService dispatchService,
            ICallService callService) {
            _callService = callService;
            _dispatchService = dispatchService;
        }
        
        protected async void Phone_OnClicked(object sender, RoutedEventArgs arg) {
            if (sender is Hyperlink) {
                var text = ((Hyperlink)sender).Inlines.Where(e => e is Run).Select(e => ((Run)e).Text).Aggregate("", (e1, e2) => e1 + e2);
                var phone = StringExtension.OnlyNumbers(text);
                await _callService?.StartCallAsync(_callService.ToPhoneNumber(phone));
            }
        }
    }
}
