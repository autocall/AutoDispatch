namespace AutoDispatch.Web.Logic.Dtos.CallReport;

/// <summary>
///     Фильтр Call Report и Phone channel congestion (CDR Vodia за день). CompanyId и User, которых нет в списках дня, не учитываются </summary>
public class CallReportFilterDto {
    /// <summary>
    ///     День CDR-файла (дата в часовом поясе PBX); пусто — сегодня по времени сервера </summary>
    public DateTime? Date { get; set; }
    public Guid? CompanyId { get; set; }
    /// <summary>
    ///     SipLogin (extension) — только для congestion </summary>
    public string User { get; set; }
    public bool OnlyOut { get; set; }

    public DateTime Day => (this.Date ?? DateTime.Now).Date;
}

/// <summary>
///     Звонок из CDR: extension и время по логу PBX </summary>
public class CdrCall {
    public string SipLogin { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class CallReportCompanyDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class CallReportUserDto {
    public string SipLogin { get; set; }
    /// <summary>
    ///     Имя пользователя AutoDispatch; extension без пользователя — сам extension </summary>
    public string UserName { get; set; }
}

/// <summary>
///     Интервал с постоянным числом одновременных звонков </summary>
public class CongestionPeriodDto {
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int Calls { get; set; }
}

/// <summary>
///     Phone channel congestion: интервалы наибольшей загрузки и итоги дня; Error — CDR не получен </summary>
public class CongestionDto {
    public List<CongestionPeriodDto> Periods { get; set; } = [];
    public List<CallReportUserDto> Users { get; set; } = [];
    public List<CallReportCompanyDto> Companies { get; set; } = [];
    public int Calls { get; set; }
    /// <summary>
    ///     Секунды </summary>
    public int Duration { get; set; }
    public string Error { get; set; }
}

public class CallReportRowDto {
    public string SipLogin { get; set; }
    public string UserName { get; set; }
    public string CompanyName { get; set; }
    public int Calls { get; set; }
    /// <summary>
    ///     Секунды </summary>
    public int Duration { get; set; }
    /// <summary>
    ///     Как в V1: 5, 6:10, 1:02:03 </summary>
    public string DurationText => TimeSpan.FromSeconds(this.Duration).ToString(@"hh\:mm\:ss").TrimStart('0', ':');
}

/// <summary>
///     Call Report: звонки и длительность по extension за день; Error — CDR не получен </summary>
public class CallReportDto {
    public List<CallReportRowDto> Rows { get; set; } = [];
    public List<CallReportCompanyDto> Companies { get; set; } = [];
    public string Error { get; set; }
}
