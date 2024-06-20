using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;

namespace App;

public class DemoService : BackgroundService
{
    private readonly IHidHideControlService _hh;
    private readonly HidHideSetupProvider _provider;

    public DemoService(IHidHideControlService hh, HidHideSetupProvider provider)
    {
        _hh = hh;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var updates = await _provider.GetUpdateInformationAsync(stoppingToken);
        var url = await _provider.GetLatestDownloadUrl(stoppingToken);
        
        var t = _hh.ApplicationPaths.ToList();

        _hh.AddApplicationPath(@"F:\Downloads\windowsdesktop-runtime-7.0.12-win-x64.exe");
    }
}