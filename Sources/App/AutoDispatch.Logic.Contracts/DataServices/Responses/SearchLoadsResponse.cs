using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Logic.DataServices.Responses {
    public class SearchLoadsResponse : BaseResponse {
        public List<QueryLoadDto> Loads { get; set; }
    }
}
