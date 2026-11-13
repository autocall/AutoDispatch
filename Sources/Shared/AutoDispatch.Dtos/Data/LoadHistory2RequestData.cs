using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistory2RequestData : BaseEventRequestData {
        public RequestHistory2Dto Request { get; set; }

        public LoadHistory2RequestData(Guid userId) : base(userId) {
        }
    }
}
