using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of companies </summary>
    [Serializable]
    public class LoadDatCompaniesResponseData : BaseEventResponseData {
        /// <summary>
        ///     DatCompanies </summary>
        [TargetResult]
        public List<DatCompanyDto> DatCompanies { get; set; }
    }
}
