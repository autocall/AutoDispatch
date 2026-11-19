using System;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SearchActualCompaniesRequest : BaseRequest {
        public Guid UserId { get; set; }
    }
}
