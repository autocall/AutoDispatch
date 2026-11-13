using AutoDispatch.Dtos.Dtos;
using System;

namespace AutoDispatch.Dtos.Data {
    [Serializable]
    public class SaveFactoringCompanyRequestData : BaseEventRequestData {
        public Guid ActualCompanyId { get; set; }
        public string FactoringCompanyJson { get; set; }

        public SaveFactoringCompanyRequestData(Guid userId) : base(userId) {
        }
    }
}
