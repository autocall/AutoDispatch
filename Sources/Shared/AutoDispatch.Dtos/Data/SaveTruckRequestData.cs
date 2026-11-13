using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveTruckRequestData : BaseEventRequestData {
        public TruckDto Truck { get; set; }

        public SaveTruckRequestData(Guid userId) : base(userId) {
        }
    }
}
