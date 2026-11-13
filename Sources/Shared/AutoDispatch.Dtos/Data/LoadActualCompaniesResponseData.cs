using System;
using System.Collections.Generic;
using AutoDispatch.Dtos.Dtos;

namespace AutoDispatch.Dtos.Data {
    /// <summary>
    ///     Data of response list of Actual companies </summary>
    [Serializable]
    public class LoadActualCompaniesResponseData : BaseEventResponseData {
        /// <summary>
        ///     Actual Companies </summary>
        [TargetResult]
        public List<ActualCompanyDto> ActualCompanies { get; set; }
    }
}
