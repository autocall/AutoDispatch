using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationLoadChangedRequestData : BaseEventRequestData {
        public LocationDto Location { get; set; }

        public DateTime DateTime { get; set; }

        public Guid LoadId { get; set; }

        public LocationLoadChangedRequestData(Guid userId) : base(userId) {
        }
    }
}
