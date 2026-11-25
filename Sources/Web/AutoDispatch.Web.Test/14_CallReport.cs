using AutoDispatch.Web.Logic.Dtos.CallReport;
using AutoDispatch.Web.Logic.Services;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Call Report: разбор CDR Vodia и интервалы Phone channel congestion без FTP </summary>
public class CallReport_Test {

    private static readonly DateTime Day = new DateTime(2026, 10, 30);

    /// <summary>
    ///     Строка CDR из 19 полей: dir, start, end, trunk, local, remote </summary>
    private static string Line(string dir, string start, string end, string trunk = "Bandwidth", string local = "0341162",
                               string remote = "+19525922359") =>
        $"phone.myautocall.net,cid,call,from,to,{dir},extcall,20261030{start},,20261030{end},{trunk},{local},{remote},,,,,,";

    private static CdrCall Call(string start, string end) => new CdrCall {
        SipLogin = "1",
        Start = Day.Add(TimeSpan.Parse(start)),
        End = Day.Add(TimeSpan.Parse(end))
    };

    /// <summary>
    ///     Отбор как в V1: только 19 полей, с trunk, Only Out, целиком в пределах дня, extension компании в local или remote </summary>
    [Fact]
    public void Test01_ParseCalls() {
        var lines = new List<string> {
            Line("O", "100000", "100130"),
            Line("I", "110000", "110010"),
            Line("O", "120000", "120010", trunk: ""),
            Line("O", "120000", "120010") + ",extra",
            Line("O", "235950", "235959"),
            "phone.myautocall.net,cid,call,from,to,O,extcall,20261029235950,,20261030000010,Bandwidth,0341162,+1,,,,,,",
            Line("I", "130000", "130100", local: "+12027733941", remote: "\"Name\" <sip:0341162@phone.myautocall.net>"),
            Line("O", "140000", "140100", local: "0999999")
        };

        var all = CallReportService.ParseCalls(lines, Day, onlyOut: false, visible: null);
        Assert.Equal(5, all.Count);
        Assert.Equal(Day.AddHours(10), all[0].Start);
        Assert.Equal(TimeSpan.FromSeconds(90), all[0].End - all[0].Start);

        var onlyOut = CallReportService.ParseCalls(lines, Day, onlyOut: true, visible: null);
        Assert.Equal(3, onlyOut.Count);

        // своя компания — 0341162: звонок с ней в remote виден, чужой 0999999 — нет
        var company = CallReportService.ParseCalls(lines, Day, onlyOut: false, visible: ["0341162"]);
        Assert.Equal(4, company.Count);
        Assert.Contains(company, e => e.SipLogin == "+12027733941");
        Assert.DoesNotContain(company, e => e.SipLogin == "0999999");
    }

    /// <summary>
    ///     Интервалы постоянного числа звонков: стык звонков с тем же числом сливается, пустые промежутки не показываются </summary>
    [Fact]
    public void Test02_BuildPeriods() {
        var calls = new List<CdrCall> {
            Call("10:00:00", "10:10:00"),
            Call("10:05:00", "10:08:00"),
            // начинается, когда второй заканчивается: число остаётся 2
            Call("10:08:00", "10:12:00"),
            Call("11:00:00", "11:01:00"),
            // нулевой длительности не считается
            Call("12:00:00", "12:00:00")
        };

        var periods = CallReportService.BuildPeriods(calls, top: 10);

        Assert.Equal([
            (Day.Add(TimeSpan.Parse("11:00")), Day.Add(TimeSpan.Parse("11:01")), 1),
            (Day.Add(TimeSpan.Parse("10:10")), Day.Add(TimeSpan.Parse("10:12")), 1),
            (Day.Add(TimeSpan.Parse("10:05")), Day.Add(TimeSpan.Parse("10:10")), 2),
            (Day.Add(TimeSpan.Parse("10:00")), Day.Add(TimeSpan.Parse("10:05")), 1)
        ], periods.Select(e => (e.Start, e.End, e.Calls)));

        // top: самые загруженные, при равенстве — длинные; порядок — от поздних к ранним
        var top = CallReportService.BuildPeriods(calls, top: 2);
        Assert.Equal([2, 1], top.Select(e => e.Calls));
        Assert.Equal(Day.Add(TimeSpan.Parse("10:00")), top[1].Start);
    }
}
