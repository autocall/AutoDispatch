using System;

namespace AutoDispatch.Logic.DataServices.Requests {
    public class SendSetupPackageRequest : BaseRequest {
        public Guid CompanyId { get; set; }
    }
}
