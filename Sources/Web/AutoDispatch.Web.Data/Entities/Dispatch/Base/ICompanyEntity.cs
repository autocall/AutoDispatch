namespace AutoDispatch.Web.Data.Entities.Dispatch;

/// <summary>
///     Запись компании: запрос App получает только записи своей компании (аудит D1, DispatchBaseService.GetByIdAsync) </summary>
public interface ICompanyEntity {
    Guid CompanyId { get; }
}
