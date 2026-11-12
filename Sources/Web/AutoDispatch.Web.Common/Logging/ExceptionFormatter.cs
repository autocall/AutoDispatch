using System.Text;
using System.Text.RegularExpressions;

namespace AutoDispatch.Web.Common;

/// <summary>
///     Короткий текст исключения для колонки Details: цепочка «Тип: Message» по Inner и до MaxFrames кадров AutoDispatch.*,
///     начиная с самого внутреннего исключения. Полный стек остаётся в файловом логе </summary>
public static class ExceptionFormatter {

    public const int MaxFrames = 5;

    // "   at AutoDispatch.Web.Logic.Services.LogService.SearchAsync(LogFilterDto filter) in D:\...\LogService.cs:line 41"
    private static readonly Regex FrameLine = new(@"^\s*at (AutoDispatch\.[^(]+)\(.*\)(?: in (.+):line (\d+))?\s*$", RegexOptions.Compiled);

    public static string Format(Exception ex) {
        var chain = Chain(ex).ToList();
        var sb = new StringBuilder();
        for (var i = 0; i < chain.Count; i++) {
            if (i > 0) {
                sb.Append(Environment.NewLine).Append("<- ");
            }
            sb.Append(Describe(chain[i]));
        }
        var frames = new List<string>();
        foreach (var e in Enumerable.Reverse(chain)) {
            foreach (var frame in Frames(e)) {
                if (frames.Count == MaxFrames) {
                    break;
                }
                if (frames.Contains(frame) == false) {
                    frames.Add(frame);
                }
            }
        }
        foreach (var frame in frames) {
            sb.Append(Environment.NewLine).Append("   at ").Append(frame);
        }
        return sb.ToString();
    }

    #region [ private ]

    private static IEnumerable<Exception> Chain(Exception ex) {
        if (ex is AggregateException aggregate) {
            foreach (var inner in aggregate.Flatten().InnerExceptions) {
                foreach (var e in Chain(inner)) {
                    yield return e;
                }
            }
            yield break;
        }
        for (var e = ex; e != null; e = e.InnerException) {
            yield return e;
        }
    }

    /// <summary>
    ///     «SqlException (208): Invalid object name», «HttpRequestException (502): …»; SqlClient в Common не подключён — Number через reflection </summary>
    private static string Describe(Exception e) {
        var type = e.GetType();
        var code = type.Name == "SqlException" ? type.GetProperty("Number")?.GetValue(e)?.ToString()
            : e is HttpRequestException http && http.StatusCode != null ? ((int)http.StatusCode).ToString()
            : null;
        return code == null ? $"{type.Name}: {e.Message}" : $"{type.Name} ({code}): {e.Message}";
    }

    private static IEnumerable<string> Frames(Exception e) {
        if (e.StackTrace == null) {
            yield break;
        }
        foreach (var line in e.StackTrace.Split('\n')) {
            var match = FrameLine.Match(line);
            if (match.Success == false) {
                continue;
            }
            var method = match.Groups[1].Value;
            yield return match.Groups[2].Success ? $"{method} in {Path.GetFileName(match.Groups[2].Value)}:{match.Groups[3].Value}" : method;
        }
    }

    #endregion [ private ]
}
