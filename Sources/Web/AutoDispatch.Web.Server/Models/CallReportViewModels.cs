using AutoDispatch.Web.Logic.Dtos.CallReport;

namespace AutoDispatch.Web.Server.Models;

/// <summary>
///     Блок Phone channel congestion на Home </summary>
public class CongestionViewModel {
    public CallReportFilterDto Filter { get; set; }
    public CongestionDto Data { get; set; }
}

/// <summary>
///     /CallReport </summary>
public class CallReportIndexViewModel {
    public CallReportFilterDto Filter { get; set; }
    public CallReportDto Data { get; set; }
}
