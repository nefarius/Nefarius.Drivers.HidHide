<img src="assets/NSS-128x128.png" align="right" />

# Nefarius.Drivers.HidHide

[![Build status](https://ci.appveyor.com/api/projects/status/ple70ifo0y17ouu4/branch/master?svg=true)](https://ci.appveyor.com/project/nefarius/nefarius-drivers-hidhide/branch/master) ![Requirements](https://img.shields.io/badge/Requires-.NET%206-blue.svg) [![Nuget](https://img.shields.io/nuget/v/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/) [![Nuget](https://img.shields.io/nuget/dt/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/)

Managed API for configuring [HidHide](https://github.com/ViGEm/HidHide).

## Usage

Create an instance of `HidHideControlService` whenever you need it. The following methods and properties are available:

```csharp
/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
public interface IHidHideControlService
{
    /// <summary>
    ///     Gets or sets whether global device hiding is currently active or not.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    ///     Gets or sets whether the application list is inverted (from block all/allow specific to allow all/block specific).
    /// </summary>
    bool IsAppListInverted { get; set; }

    /// <summary>
    ///     Returns list of currently blocked instance IDs.
    /// </summary>
    IReadOnlyList<string> BlockedInstanceIds { get; }

    /// <summary>
    ///     Returns list of currently allowed (or blocked, see <see cref="IsAppListInverted" />) application paths.
    /// </summary>
    IReadOnlyList<string> ApplicationPaths { get; }

    /// <summary>
    ///     Submit a new instance to block.
    /// </summary>
    /// <param name="instanceId">The Instance ID to block.</param>
    void AddBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Remove an instance from being blocked.
    /// </summary>
    /// <param name="instanceId">The Instance ID to unblock.</param>
    void RemoveBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Empties the device instances list. Useful if <see cref="AddBlockedInstanceId" /> or
    ///     <see cref="BlockedInstanceIds" /> throw exceptions due to nonexistent entries.
    /// </summary>
    void ClearBlockedInstancesList();

    /// <summary>
    ///     Submit a new application to allow (or deny if inverse flag is set).
    /// </summary>
    /// <param name="path">The absolute application path to allow.</param>
    void AddApplicationPath(string path);

    /// <summary>
    ///     Revokes an applications exemption.
    /// </summary>
    /// <param name="path">The absolute application path to revoke.</param>
    void RemoveApplicationPath(string path);

    /// <summary>
    ///     Empties the application list. Useful if <see cref="AddApplicationPath" /> or <see cref="ApplicationPaths" /> throw
    ///     exceptions due to nonexistent entries.
    /// </summary>
    void ClearApplicationsList();
}
```
