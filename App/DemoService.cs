using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;
using Nefarius.Vicius.Abstractions.Models;

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
        UpdateResponse? updates = await _provider.GetUpdateInformationAsync(stoppingToken);
        Uri url = await _provider.GetLatestDownloadUrlAsync(stoppingToken);
        var version = await _provider.GetLatestVersionAsync(stoppingToken);

        List<string> t = _hh.ApplicationPaths.ToList();

        _hh.AddApplicationPath(@"F:\Downloads\windowsdesktop-runtime-7.0.12-win-x64.exe");
    }
}