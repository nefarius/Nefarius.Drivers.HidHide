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

        Assert.That(release, Is.Not.Null);
    }

    [Test]
    public async Task TestGetLatestVersionAsync()
    {
        ServiceCollection sc = new();
        sc.AddHidHide();

        ServiceProvider sp = sc.BuildServiceProvider();

        HidHideSetupProvider provider = sp.GetRequiredService<HidHideSetupProvider>();

        Version version = await provider.GetLatestVersionAsync();

        Assert.That(version, Is.EqualTo(Version.Parse("1.5.230.0")));
    }

    [Test]
    public async Task TestDownloadLatestReleaseAsync()
    {
        ServiceCollection sc = new();
        sc.AddHidHide();

        ServiceProvider sp = sc.BuildServiceProvider();

        HidHideSetupProvider provider = sp.GetRequiredService<HidHideSetupProvider>();

        HttpResponseMessage response = await provider.DownloadLatestReleaseAsync();

        Assert.That(response.IsSuccessStatusCode, Is.True);

        byte[] body = await response.Content.ReadAsByteArrayAsync();
        
        Assert.That(body, Is.Not.Empty);
    }
}