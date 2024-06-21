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
    ///     Registers <see cref="HidHideControlService" /> with DI.
    /// </summary>
    public static IServiceCollection AddHidHide(this IServiceCollection services,
        Action<HidHideServiceOptions>? options = default)
    {
        HidHideServiceOptions serviceOptions = new();

        options?.Invoke(serviceOptions);

        services.TryAddSingleton<IHidHideControlService, HidHideControlService>();

        services.AddHttpClient<HidHideSetupProvider>(client =>
        {
            client.BaseAddress = new Uri("https://vicius.api.nefarius.systems/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(nameof(HidHideSetupProvider));
            client.DefaultRequestHeaders.Add("X-Vicius-OS-Architecture",
                serviceOptions.ProcessArchitecture.ToString().ToLowerInvariant());
        });

        return services;
    }
}