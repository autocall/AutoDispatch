using AutoCall.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDispatch.Dtos.Dtos {
    public class CompanyCustomStatusDto : BaseDto {
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
    }
}
