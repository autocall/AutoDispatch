using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class HistoryLocationDriverViewModel : BaseHistoryLoationViewModel {
        public Guid DriverId { get; set; }

        public int? HistoryLocationTruckId { get; set; }
    }
}
