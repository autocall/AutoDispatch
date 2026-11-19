using AutoDispatch.Dtos.Dtos;
using System.Collections.Generic;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class GetFactoringCompaniesResponse : BaseResponse {
        public List<SetupPacketFactoringCompanyDto> Data { get; set; }
    }
}
