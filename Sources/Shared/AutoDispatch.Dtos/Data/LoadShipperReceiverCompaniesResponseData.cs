using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadShipperReceiverCompaniesResponseData : BaseEventResponseData {
        [TargetResult]
        public List<ShipperReceiverCompanyDto> Companies { get; set; }
    }
}
