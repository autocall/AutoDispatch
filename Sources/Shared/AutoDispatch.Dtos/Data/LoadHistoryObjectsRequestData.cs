using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistoryObjectsRequestData : BaseEventRequestData {
        public List<int> Ids { get; set; }

        public LoadHistoryObjectsRequestData(Guid userId) : base(userId) {
        }
    }
}
