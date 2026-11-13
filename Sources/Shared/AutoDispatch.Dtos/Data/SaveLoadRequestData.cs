using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveLoadRequestData : BaseEventRequestData {
        public LoadDto Load { get; set; }

        public bool SaveInvoice { get; set; }

        public SaveLoadRequestData(Guid userId) : base(userId) {
        }
    }
}
