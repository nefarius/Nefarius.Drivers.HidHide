using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;
using Nefarius.Vicius.Abstractions.Models;

namespace App;

public class DemoService : BackgroundService
{
    // injects the service used to configure HidHide
    private readonly IHidHideControlService _hh;
    // injects the service for web communication (updates, setup etc.)
    private readonly HidHideSetupProvider _provider;

    public DemoService(IHidHideControlService hh, HidHideSetupProvider provider)
    {
        _hh = hh;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        #region These calls require Internet access
        
        // demos how to get the latest release details
        UpdateRelease release = await _provider.GetLatestReleaseAsync(stoppingToken);
        // demos how to get the latest setup download URL
        // this takes the processor architecture of this machine into account
        Uri url = await _provider.GetLatestDownloadUrlAsync(stoppingToken);
        // demos how to get the latest available version
        Version version = await _provider.GetLatestVersionAsync(stoppingToken);
        
        #endregion

        #region These calls are issued to the local driver
        
        List<string> apps = _hh.ApplicationPaths.ToList();

        // this path must exist locally or will throw an exception
        _hh.AddApplicationPath(@"F:\Downloads\windowsdesktop-runtime-7.0.12-win-x64.exe");
        
        #endregion
    }
}