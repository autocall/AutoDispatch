using AutoDispatch.Dtos.Dtos;
using System;
using System.Collections.Generic;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class LoadHistory2ResponseData : BaseEventResponseData {
        public List<LoadDto> Loads { get; set; }
        public List<DriverDto> Drivers { get; set; }
        public List<TruckDto> Trucks { get; set; }
        public List<TrailerDto> Trailers { get; set; }
        public List<WarehouseDto> Warehouses { get; set; }
        public List<YardDto> Yards { get; set; }
        public List<DatCompanyDto> DatCompanies { get; set; }
        public List<ActualCompanyDto> ActualCompanies { get; set; }
        public int Total { get; set; }
        public int PageIndex { get; set; }
    }
}
