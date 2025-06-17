using Nefarius.Vicius.Abstractions.Models;

namespace Tests;

internal partial class Tests
{
    [Test]
    public async Task TestGetLatestReleaseAsync()
    {
        UpdateRelease release = await _hhProvider.GetLatestReleaseAsync();

        Assert.That(release, Is.Not.Null);
    }

    [Test]
    public async Task TestGetLatestVersionAsync()
    {
        Version version = await _hhProvider.GetLatestVersionAsync();

        Assert.That(version, Is.EqualTo(Version.Parse("1.5.230.0")));
    }

    [Test]
    public async Task TestDownloadLatestReleaseAsync()
    {
        HttpResponseMessage response = await _hhProvider.DownloadLatestReleaseAsync();

        Assert.That(response.IsSuccessStatusCode, Is.True);

        byte[] body = await response.Content.ReadAsByteArrayAsync();

        Assert.That(body, Is.Not.Empty);
    }
}