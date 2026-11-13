using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveDriverRequestData : BaseEventRequestData {
        public DriverDto Driver { get; set; }

        public SaveDriverRequestData(Guid userId) : base(userId) {
        }
    }
}
