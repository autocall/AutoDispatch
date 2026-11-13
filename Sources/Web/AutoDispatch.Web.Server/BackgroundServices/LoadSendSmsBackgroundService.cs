using AutoDispatch.Web.Common;
using AutoDispatch.Web.Logic;
using AutoDispatch.Web.Logic.Services.Dispatch;

namespace AutoDispatch.Web.Server.BackgroundServices;

/// <summary>
///     V1 LoadSendSmsJob (5 мин): SMS складу о грузах из WA (AppSettings.LoadSendSms*) </summary>
public class LoadSendSmsBackgroundService : BaseBackgroundService {

    protected override TimeSpan Interval => AppSettings.LoadSendSmsInterval;
    protected override string Name => "load-send-sms";

    public LoadSendSmsBackgroundService(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override Task ExecuteIterationAsync(ServicesHost host, CancellationToken stoppingToken) {
        return host.GetService<DispatchService>().LoadSendSms_Process();
    }
}
