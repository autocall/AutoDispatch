using System;
using AutoDispatch.Logic.ViewModels.Items;

namespace AutoDispatch.Logic.ViewModels {
    public class BaseHistoryLoationViewModel {
        public int Id { get; set; }

        public LocationViewModel Location { get; set; }

        public int EventId { get; set; }

        public int HistoryId { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid CreatedById { get; set; }
    }
}
