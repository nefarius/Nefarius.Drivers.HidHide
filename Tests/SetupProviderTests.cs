using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Nefarius.Drivers.HidHide;
using Nefarius.Drivers.HidHide.Exceptions;

namespace Tests;

internal sealed class SetupProviderTests
{
    private const string BaseUrl = "https://vicius.api.nefarius.systems/";

    private static string UpdatesJson(params string[] releases) =>
        $$"""{"instance":{},"shared":{},"releases":[{{string.Join(",", releases)}}]}""";

    private static string ReleaseJson(string version, string? downloadUrl)
    {
        string url = downloadUrl is null ? "null" : $"\"{downloadUrl}\"";

        return $$"""{"name":"HidHide {{version}}","version":"{{version}}","summary":"s","publishedAt":"2026-01-01T00:00:00Z","downloadUrl":{{url}}}""";
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_PicksHighestVersion()
    {
        RecordingHandler handler = new(HttpStatusCode.OK,
            UpdatesJson(ReleaseJson("1.4.0.0", "https://cdn.example/old.exe"),
                ReleaseJson("1.6.0.0", "https://cdn.example/new.exe"),
                ReleaseJson("1.5.0.0", "https://cdn.example/mid.exe")));

        HidHideSetupProvider provider = CreateProvider(handler);

        Version version = await provider.GetLatestVersionAsync();

        Assert.That(version, Is.EqualTo(new Version(1, 6, 0, 0)));
        Assert.That((await provider.GetLatestReleaseAsync()).DownloadUrl,
            Is.EqualTo("https://cdn.example/new.exe"));
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_NoReleases_ThrowsMissingReleases()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK, UpdatesJson()));

        await Assert.ThrowsAsync<MissingReleasesException>(() => provider.GetLatestReleaseAsync());
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_NullBody_ThrowsUpdateResponseMissing()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK, "null"));

        await Assert.ThrowsAsync<UpdateResponseMissingException>(() => provider.GetLatestReleaseAsync());
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_ServerError_ThrowsUpdateResponseMissingWithInnerException()
    {
        HidHideSetupProvider provider =
            CreateProvider(new RecordingHandler(HttpStatusCode.InternalServerError, "boom"));

        UpdateResponseMissingException? ex = await Assert.ThrowsAsync<UpdateResponseMissingException>(() => provider.GetLatestReleaseAsync());

        Assert.That(ex!.InnerException, Is.Not.Null);
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_MalformedJson_ThrowsUpdateResponseMissingWithInnerException()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK, "{not json"));

        UpdateResponseMissingException? ex = await Assert.ThrowsAsync<UpdateResponseMissingException>(() => provider.GetLatestReleaseAsync());

        Assert.That(ex!.InnerException, Is.Not.Null);
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestDownloadUrl_ReturnsAbsoluteUri()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK,
            UpdatesJson(ReleaseJson("1.0.0.0", "https://cdn.example/setup.exe"))));

        Uri uri = await provider.GetLatestDownloadUrlAsync();

        Assert.That(uri, Is.EqualTo(new Uri("https://cdn.example/setup.exe")));
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestDownloadUrl_MissingUrl_Throws()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK,
            UpdatesJson(ReleaseJson("1.0.0.0", null))));

        await Assert.ThrowsAsync<DownloadLocationMissingException>(() => provider.GetLatestDownloadUrlAsync());
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestDownloadUrl_RelativeUrl_Throws()
    {
        HidHideSetupProvider provider = CreateProvider(new RecordingHandler(HttpStatusCode.OK,
            UpdatesJson(ReleaseJson("1.0.0.0", "setup.exe"))));

        await Assert.ThrowsAsync<MalformedUrlException>(() => provider.GetLatestDownloadUrlAsync());
    }

    [Test]
    [Category("Unit")]
    public async Task DownloadLatestRelease_RequestsReleaseUrl()
    {
        RecordingHandler handler = new(HttpStatusCode.OK,
            UpdatesJson(ReleaseJson("1.0.0.0", "https://cdn.example/setup.exe")),
            new byte[] { 1, 2, 3 });

        HidHideSetupProvider provider = CreateProvider(handler);

        HttpResponseMessage response = await provider.DownloadLatestReleaseAsync();

        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(await response.Content.ReadAsByteArrayAsync(), Is.EqualTo(new byte[] { 1, 2, 3 }));
        Assert.That(handler.Requests.Last().RequestUri, Is.EqualTo(new Uri("https://cdn.example/setup.exe")));
    }

    [Test]
    [Category("Unit")]
    public async Task GetLatestRelease_CanceledToken_Throws()
    {
        RecordingHandler handler = new(HttpStatusCode.OK, UpdatesJson(ReleaseJson("1.0.0.0", null)));
        handler.BlockUntilCanceled = true;

        HidHideSetupProvider provider = CreateProvider(handler);

        using CancellationTokenSource cts = new();
        cts.Cancel();

        UpdateResponseMissingException? ex =
            await Assert.ThrowsAsync<UpdateResponseMissingException>(() => provider.GetLatestReleaseAsync(cts.Token));

        Assert.That(ex!.InnerException, Is.InstanceOf<TaskCanceledException>());
    }

    private static HidHideSetupProvider CreateProvider(RecordingHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) });

    /// <summary>
    ///     Serves canned responses and records the requests it saw.
    /// </summary>
    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly byte[] _body;
        private readonly byte[]? _secondBody;
        private int _calls;

        public RecordingHandler(HttpStatusCode status, string json, byte[]? secondBody = null)
        {
            Status = status;
            _body = Encoding.UTF8.GetBytes(json);
            _secondBody = secondBody;
        }

        public HttpStatusCode Status { get; }

        public bool BlockUntilCanceled { get; set; }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (BlockUntilCanceled)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            byte[] body = _calls++ == 0 || _secondBody is null ? _body : _secondBody;

            return new HttpResponseMessage(Status) { Content = new ByteArrayContent(body) };
        }
    }
}
