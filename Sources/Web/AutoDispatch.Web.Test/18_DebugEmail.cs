#if DEBUG
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Xunit.Abstractions;
using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic.Email;

namespace AutoDispatch.Web.Test;

/// <summary>
///     Аудит D8: в Debug письмо компании (EmailUtils) уходит только на AppSettings.DebugEmailRecipient, настоящие получатели — в теме.
///     Без сервера и без отправки наружу: SMTP — локальный приёмник в тесте, записывает RCPT TO и текст письма </summary>
public class DebugEmail_Test {

    private readonly ITestOutputHelper _output;

    public DebugEmail_Test(ITestOutputHelper output) {
        this._output = output;
    }

    [Fact]
    public async Task Test01_SmtpRedirectedToDebugRecipient() {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var recipients = new List<string>();
        var data = new StringBuilder();
        var server = Task.Run(async () => {
            using var client = await listener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true, NewLine = "\r\n" };
            await writer.WriteLineAsync("220 test");
            var inData = false;
            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                if (inData) {
                    if (line == ".") {
                        inData = false;
                        await writer.WriteLineAsync("250 OK");
                    } else {
                        data.Append(line).Append('\n');
                    }
                } else if (line.StartsWith("RCPT TO:")) {
                    recipients.Add(line.Substring(8).Trim('<', '>', ' '));
                    await writer.WriteLineAsync("250 OK");
                } else if (line == "DATA") {
                    inData = true;
                    await writer.WriteLineAsync("354 go");
                } else if (line == "QUIT") {
                    await writer.WriteLineAsync("221 bye");
                    break;
                } else {
                    await writer.WriteLineAsync("250 test");
                }
            }
        });
        try {
            var email = new Email("carrier@example.com", "company@example.com", "Setup Packet", "<p>test</p>") { IsBodyHTML = true };
            email.CcEmailAddresses.Add(new EmailAddress("cc@example.com"));
            email.BccEmailAddresses.Add(new EmailAddress("bcc@example.com"));
            await EmailUtils.SendEmailAsync(new SmtpSettings("user", "password", "127.0.0.1", port) { EnableSsl = false }, email);
            await server.WaitAsync(TimeSpan.FromSeconds(10));
        } finally {
            listener.Stop();
        }

        // заголовки длиннее строки переносятся с отступом — склеить
        var headers = Regex.Replace(data.ToString(), "\n[ \t]+", " ");
        var subject = Regex.Match(headers, "^Subject: (.*)$", RegexOptions.Multiline).Groups[1].Value;
        _output.WriteLine($"RCPT TO: {String.Join(", ", recipients)}");
        _output.WriteLine($"Subject: {subject}");
        Assert.Equal(new[] { AppSettings.DebugEmailRecipient }, recipients);
        Assert.Equal("[DEBUG to: carrier@example.com, cc@example.com, bcc@example.com] Setup Packet", subject);
        Assert.DoesNotContain("example.com", Regex.Match(headers, "^(To|Cc): (.*)$", RegexOptions.Multiline).Value);
    }
}
#endif
