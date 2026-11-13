using AutoDispatch.Dtos.Dtos;
using AutoDispatch.Web.Common.Exceptions;
using AutoDispatch.Web.Logic.WebServices;

namespace AutoDispatch.Web.Logic.Services.Dispatch;

/// <summary>
///     Поиск компаний FMCSA через Safer api/v3/companies (V1 CensusService): Api/Census App и DOT брокера при сохранении груза </summary>
public class CensusService : BaseService {

    public CensusService(ServicesHost host) : base(host) { }

    public async Task<List<CensusCompanyDto>> SearchCensusCompaniesAsync(CensusCompanySearchDto search) {
        var result = await Host.GetService<BrokersnapshotWebService>().CompaniesAsync(Convert(search));
        if (result.Success) {
            return Mapper.Map<List<CensusCompanyDto>>(result.Data);
        }
        throw new InfoException(result.ErrorMessage);
    }

    private static Dictionary<string, string> Convert(CensusCompanySearchDto search) {
        var result = new Dictionary<string, string>();
        result["Limit"] = search.Limit.ToString();
        if (search.DOT_NUMBER != null) result["DOT"] = search.DOT_NUMBER.ToString();
        if (search.NAME != null) result["Name"] = search.NAME;
        if (search.PHY_STR != null) result["Address"] = search.PHY_STR;
        if (search.PHY_CITY != null) result["City"] = search.PHY_CITY;
        if (search.PHY_ST != null) result["State"] = search.PHY_ST;
        if (search.TEL_NUM != null) result["Contact"] = search.TEL_NUM;
        if (search.CELL_NUM != null) result["Contact"] = search.CELL_NUM;
        if (search.MAI_STR != null) result["Address"] = search.MAI_STR;
        if (search.MAI_CITY != null) result["City"] = search.MAI_CITY;
        if (search.MAI_ST != null) result["State"] = search.MAI_ST;
        if (search.ICC_DOCKET_PREFIX != null) result["Prefix"] = search.ICC_DOCKET_PREFIX;
        if (search.ICC != null) result["Docket"] = search.ICC.ToString();
        return result;
    }
}
