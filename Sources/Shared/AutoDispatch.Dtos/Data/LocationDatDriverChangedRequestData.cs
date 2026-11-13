using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LocationDatDriverChangedRequestData : BaseEventRequestData {
        public LocationDto Location { get; set; }

        public DateTime DateTime { get; set; }

        public Guid DatDriverId { get; set; }

        public LocationDatDriverChangedRequestData(Guid userId) : base(userId) {
        }
    }
}
