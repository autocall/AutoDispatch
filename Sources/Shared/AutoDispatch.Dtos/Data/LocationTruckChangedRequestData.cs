using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationTruckChangedRequestData : BaseEventRequestData {
        public LocationDto Location { get; set; }

        public DateTime DateTime { get; set; }

        public Guid TruckId { get; set; }

        public LocationTruckChangedRequestData(Guid userId) : base(userId) {
        }
    }
}
