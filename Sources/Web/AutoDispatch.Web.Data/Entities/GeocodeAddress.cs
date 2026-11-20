using System.ComponentModel.DataAnnotations.Schema;

namespace AutoDispatch.Web.Data.Entities;

/// <summary>
///     Кэш обратного геокодинга (V1 — адреса в общей таблице Cache, ключ 32 байта): точка округлена с шагом 0.0005,
///     Lat/Lng = round(координата / 0.0005). Address — formatted_address, пустая строка — адреса нет (ZERO_RESULTS).
///     Заполняется GeocodeService и первичным переносом из autocall.dbo.Cache (решено 2026-11-20, AI/wiki/plan.md) </summary>
public class GeocodeAddress {

    public int Lat { get; set; }

    public int Lng { get; set; }

    [Column(TypeName = "varchar(400)")]
    public string Address { get; set; }

    [Column(TypeName = "datetime2(0)")]
    public DateTime CreatedDate { get; set; }
}
