using System.Runtime.InteropServices;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Optional options for HidHide service registration.
/// </summary>
public sealed class HidHideServiceOptions
{
    internal HidHideServiceOptions() { }

    /// <summary>
    ///     The processor/machine architecture to report to the CDN server.
    /// </summary>
    public Architecture ProcessArchitecture { get; set; } = RuntimeInformation.ProcessArchitecture;
}