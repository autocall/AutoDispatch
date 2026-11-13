using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationDriverChangedRequestData : BaseEventRequestData {
        public LocationDto Location { get; set; }

        public DateTime DateTime { get; set; }

        public Guid DriverId { get; set; }

        public LocationDriverChangedRequestData(Guid userId) : base(userId) {
        }
    }
}
