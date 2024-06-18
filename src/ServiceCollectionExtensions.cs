using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Service collection extension methods.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers <see cref="HidHideControlService"/> with DI.
    /// </summary>
    public static IServiceCollection AddHidHide(this IServiceCollection services)
    {
        services.TryAddSingleton<IHidHideControlService, HidHideControlService>();
        
        return services;
    }
}