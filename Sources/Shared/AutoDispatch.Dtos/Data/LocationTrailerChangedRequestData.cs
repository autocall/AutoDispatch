using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationTrailerChangedRequestData : BaseEventRequestData {
        public LocationDto Location { get; set; }

        public DateTime DateTime { get; set; }

        public Guid TrailerId { get; set; }

        public LocationTrailerChangedRequestData(Guid userId) : base(userId) {
        }
    }
}
