using Microsoft.Extensions.Hosting;

using Nefarius.Drivers.HidHide;

namespace App;

public class DemoService : BackgroundService
{
    private readonly IHidHideControlService _hh;

    public DemoService(IHidHideControlService hh)
    {
        _hh = hh;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var t = _hh.ApplicationPaths.ToList();

        _hh.AddApplicationPath(@"F:\Downloads\windowsdesktop-runtime-7.0.12-win-x64.exe");
    }
}