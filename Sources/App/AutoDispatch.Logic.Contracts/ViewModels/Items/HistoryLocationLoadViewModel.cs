using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class HistoryLocationLoadViewModel : BaseHistoryLoationViewModel {

        public Guid LoadId { get; set; }

        public int? HistoryLocationTrailerId { get; set; }
    }
}
