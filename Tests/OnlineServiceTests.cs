using Microsoft.Extensions.DependencyInjection;

using Nefarius.Drivers.HidHide;
using Nefarius.Vicius.Abstractions.Models;

namespace Tests;

internal partial class Tests
{
    [Test]
    public async Task TestGetLatestReleaseAsync()
    {
        ServiceCollection sc = new();
        sc.AddHidHide();

        ServiceProvider sp = sc.BuildServiceProvider();

        HidHideSetupProvider provider = sp.GetRequiredService<HidHideSetupProvider>();

        UpdateRelease release = await provider.GetLatestReleaseAsync();
        
        Assert.Pass();
    }
}