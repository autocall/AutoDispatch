using AutoCall.Common.Extensions;
using AutoDispatch.Dtos.Enums;

namespace AutoDispatch.Logic.ViewModels.Items {
    public class CensusCompanySearchViewModel {
        private string _tEL_NUM;
        private string _cELL_NUM;

        public int Limit { get; set; }

        public string ACT_STAT { get; set; }
        public string[] CARSHIP { get; set; }
        public int? DOT_NUMBER { get; set; }
        public string NAME { get; set; }
        public string PHY_NATN { get; set; }
        public string PHY_STR { get; set; }
        public string PHY_CITY { get; set; }
        public string PHY_ST { get; set; }
        public string TEL_NUM {
            get => _tEL_NUM;
            set => _tEL_NUM = StringExtension.OnlyNumbers(value);
        }
        public string CELL_NUM {
            get => _cELL_NUM;
            set => _cELL_NUM = StringExtension.OnlyNumbers(value);
        }
        public string MAI_NATN { get; set; }
        public string MAI_STR { get; set; }
        public string MAI_CITY { get; set; }
        public string MAI_ST { get; set; }
        public string ICC_DOCKET_PREFIX { get; set; }
        public int? ICC { get; set; }
        public int? TOT_PWR_MIN { get; set; }
        public int? TOT_PWR_MAX { get; set; }
        public int? TOT_DRS_MIN { get; set; }
        public int? TOT_DRS_MAX { get; set; }
        public string RATING { get; set; }
        public string RATEDATE { get; set; }
        public string EMAILADDRESS { get; set; }
        public string COMPANY_REP1 { get; set; }

        public DatCompanyType CompanyType {
            set {
                switch (value) {
                    case DatCompanyType.Broker:
                        CARSHIP = new string[] { "B", "BI", "CB", "CBI", "CFB", "CSB", "CTB", "FB", "FBI" };
                        break;
                    case DatCompanyType.Carrier:
                        CARSHIP = new string[] { "C", "CBI", "CF", "CFB", "CFI", "CI", "CR", "CS", "CSB", "CST", "CT", "CTB", "CTTT" };
                        break;
                }
            }
        }
    }
}
