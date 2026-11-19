using System;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class HistoryObjectViewModel {
        public int Id { get; set; }

        public byte[] Data { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid ReferenceId { get; set; }
    }
}
