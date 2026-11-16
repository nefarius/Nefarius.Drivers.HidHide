using System.Net;
using System.Runtime.InteropServices;

using Microsoft.Extensions.DependencyInjection;

using Nefarius.Drivers.HidHide;

namespace Tests;

internal sealed class ServiceCollectionExtensionsTests
{
    [Test]
    [Category("Unit")]
    public void AddHidHide_ResolvesBothServices()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddHidHide()
            .BuildServiceProvider();

        Assert.That(provider.GetService<IHidHideControlService>(), Is.Not.Null);
        Assert.That(provider.GetService<HidHideSetupProvider>(), Is.Not.Null);
    }

    [Test]
    [Category("Unit")]
    public async Task AddHidHide_ConfiguresHttpClient()
    {
        CapturingHandler handler = new();

        ServiceProvider provider = new ServiceCollection()
            .AddHidHide(builder: clientBuilder => clientBuilder.ConfigurePrimaryHttpMessageHandler(() => handler))
            .BuildServiceProvider();

        HidHideSetupProvider setup = provider.GetRequiredService<HidHideSetupProvider>();

        await setup.GetLatestReleaseAsync();

        HttpRequestMessage request = handler.Requests.Single();

        Assert.That(request.RequestUri,
            Is.EqualTo(new Uri("https://vicius.api.nefarius.systems/api/nefarius/HidHide/updates.json")));
        Assert.That(request.Headers.UserAgent.ToString(), Does.Contain(nameof(HidHideSetupProvider)));
        Assert.That(request.Headers.GetValues("X-Vicius-OS-Architecture").Single(),
            Is.EqualTo(RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()));
    }

    [Test]
    [Category("Unit")]
    public async Task AddHidHide_HonorsCustomArchitecture()
    {
        CapturingHandler handler = new();

        ServiceProvider provider = new ServiceCollection()
            .AddHidHide(
                options => options.OSArchitecture = Architecture.Arm64,
                clientBuilder => clientBuilder.ConfigurePrimaryHttpMessageHandler(() => handler))
            .BuildServiceProvider();

        await provider.GetRequiredService<HidHideSetupProvider>().GetLatestReleaseAsync();

        Assert.That(handler.Requests.Single().Headers.GetValues("X-Vicius-OS-Architecture").Single(),
            Is.EqualTo("arm64"));
    }

    [Test]
    [Category("Unit")]
    public void AddHidHide_InvokesBuilderCallback()
    {
        bool invoked = false;

        new ServiceCollection().AddHidHide(builder: _ => invoked = true);

        Assert.That(invoked, Is.True);
    }

    [Test]
    [Category("Unit")]
    public void AddHidHide_DoesNotOverrideExistingControlService()
    {
        StubControlService stub = new();

        ServiceProvider provider = new ServiceCollection()
            .AddSingleton<IHidHideControlService>(stub)
            .AddHidHide()
            .BuildServiceProvider();

        Assert.That(provider.GetRequiredService<IHidHideControlService>(), Is.SameAs(stub));
    }

    /// <summary>
    ///     Answers every request with an empty releases payload and records it.
    /// </summary>
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"instance":{},"shared":{},"releases":[]}""")
            });
        }
    }

    private sealed class StubControlService : IHidHideControlService
    {
        public bool IsActive { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public bool IsAppListInverted { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IReadOnlyList<string> ApplicationPaths => throw new NotSupportedException();
        public IReadOnlyList<string> BlockedInstanceIds => throw new NotSupportedException();
        public bool IsInstalled => throw new NotSupportedException();
        public bool IsDriverNodePresent => throw new NotSupportedException();
        public bool IsOperational => throw new NotSupportedException();
        public Version? LocalDriverVersion => throw new NotSupportedException();

        public void AddApplicationPath(string path, bool throwIfNotExists = true) => throw new NotSupportedException();
        public void RemoveApplicationPath(string path) => throw new NotSupportedException();
        public void ClearApplicationsList() => throw new NotSupportedException();
        public void AddBlockedInstanceId(string instanceId) => throw new NotSupportedException();
        public void RemoveBlockedInstanceId(string instanceId) => throw new NotSupportedException();
        public void ClearBlockedInstancesList() => throw new NotSupportedException();
    }
}
