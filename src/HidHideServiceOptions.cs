using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Optional options for HidHide service registration.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class HidHideServiceOptions
{
    internal HidHideServiceOptions() { }

    /// <summary>
    ///     The processor/machine architecture to report to the CDN server.
    /// </summary>
    public Architecture OSArchitecture { get; set; } = RuntimeInformation.OSArchitecture;
}