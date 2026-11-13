using AutoCall.Common.Enums;
using System;

namespace AutoDispatch.Dtos.Dtos {
    public class RequestHistory2Dto {
        public Guid? ActualCompanyId { get; set; }

        public DateTime? MinPickupDate { get; set; }
        public DateTime? MaxPickupDate { get; set; }
        public DateTime? MinDeliveryDate { get; set; }
        public DateTime? MaxDeliveryDate { get; set; }

        public LocationDto Origin { get; set; }
        public LocationDto Destination { get; set; }
        public int? OriginRadius { get; set; }
        public int? DestinationRadius { get; set; }

        public int? MinPallets { get; set; }
        public int? MaxPallets { get; set; }
        public int? MinWeight { get; set; }
        public int? MaxWeight { get; set; }
        public int? MinLineHaul { get; set; }
        public int? MaxLineHaul { get; set; }

        public string LoadId { get; set; }
        public string ProId { get; set; }

        public AutoCallEquipmentType? Equipment { get; set; }
        public int? LoadType { get; set; }
        public string BrokerName { get; set; }
        public string CreatedBy { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
