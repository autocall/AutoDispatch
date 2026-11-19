using System;
using System.Collections.Generic;
using AutoCall.Common.Extensions;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels.Pages {
    public class SendSmsViewModel : ViewModel {
        public string ToPhone { get; set; }
        public string Message { get; set; }
        public Guid ActualCompanyId { get; set; }
        public List<DriverViewModel> Drivers { get; set; }
        public DriverViewModel SelectedDriver { get; set; }

        public string ResultPhone {
            get {
                if (SelectedDriver != null) {
                    return StringExtension.OnlyNumbers(SelectedDriver.Phone);
                }
                return StringExtension.OnlyNumbers(ToPhone);
            }
        }
    }
}
