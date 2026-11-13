using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDatDriverRequestData : BaseEventRequestData {
        public DatDriverDto DatDriver { get; set; }

        public SaveDatDriverRequestData(Guid userId) : base(userId) {
        }
    }
}
