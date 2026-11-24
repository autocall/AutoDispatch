using AutoCall.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AutoDispatch.Dtos.Dtos {
    // [DataMember] добавлены 2026-11-24 (решение пользователя): BaseDto — [DataContract], без них Newtonsoft не писал и не читал
    // Name/CompanyId — App получал статусы при входе (SessionDto.CustomStatuses) без имён; в V1 сервер отдавал словарь id→имя вручную
    public class CompanyCustomStatusDto : BaseDto {
        [DataMember] public string Name { get; set; }
        [DataMember] public Guid CompanyId { get; set; }
    }
}
