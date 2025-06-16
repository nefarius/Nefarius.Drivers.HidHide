#nullable enable

using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Service collection extension methods.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers <see cref="HidHideControlService" /> and <see cref="HidHideSetupProvider"/> with DI.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to modify.</param>
    /// <param name="options">Optional options to customize the registered HidHide services.</param>
    /// <param name="builder">
    ///     Optional <see cref="IHttpClientBuilder" /> to e.g. add resiliency policies or further customize
    ///     the named HTTP client.
    /// </param>
    public static IServiceCollection AddHidHide(this IServiceCollection services,
        Action<HidHideServiceOptions>? options = null, Action<IHttpClientBuilder>? builder = null)
    {
        HidHideServiceOptions serviceOptions = new();

        options?.Invoke(serviceOptions);

        services.TryAddSingleton<IHidHideControlService, HidHideControlService>();

        IHttpClientBuilder clientBuilder = services.AddHttpClient<HidHideSetupProvider>(client =>
        {
            client.BaseAddress = new Uri("https://vicius.api.nefarius.systems/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(nameof(HidHideSetupProvider));
            client.DefaultRequestHeaders.Add("X-Vicius-OS-Architecture",
                serviceOptions.OSArchitecture.ToString().ToLowerInvariant());
        });

        builder?.Invoke(clientBuilder);

        return services;
    }
}