# <img src="assets/NSS-128x128.png" align="left" />Nefarius.Drivers.HidHide

[![.NET](https://github.com/nefarius/Nefarius.Drivers.HidHide/actions/workflows/build.yml/badge.svg)](https://github.com/nefarius/Nefarius.Drivers.HidHide/actions/workflows/build.yml) ![Requirements](https://img.shields.io/badge/Requires-.NET%206%2F7%2F8-blue.svg) ![Requirements](https://img.shields.io/badge/Requires-.NET%20Standard%202.0-blue.svg) [![Nuget](https://img.shields.io/nuget/v/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/) [![Nuget](https://img.shields.io/nuget/dt/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/)

Managed API for configuring [HidHide](https://github.com/nefarius/HidHide).

## Usage

Create an instance of `HidHideControlService` whenever you need it.

You can use it as a singleton with dependency injection as well.

This class **will not block other configuration apps** so you can keep it in memory as long as you need. A handle to the
driver is only opened when necessary (when the properties are read from or the methods get invoked).

The following methods and properties are available:

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
    ///     Gets whether the driver is present and operable.
    /// </summary>
    bool IsInstalled { get; }

    /// <summary>
    ///     Gets or sets whether the application list is inverted (from block all/allow specific to allow all/block specific).
    /// </summary>
    /// <remarks>
    ///     The default behaviour of the application list is to block all processes by default and only treat listed paths
    ///     as exempted.
    /// </remarks>
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
    /// <remarks>
    ///     To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
    ///     https://github.com/nefarius/Nefarius.Utilities.DeviceManagement
    /// </remarks>
    /// <param name="instanceId">The Instance ID to block.</param>
    void AddBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Remove an instance from being blocked.
    /// </summary>
    /// <remarks>
    ///     To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
    ///     https://github.com/nefarius/Nefarius.Utilities.DeviceManagement
    /// </remarks>
    /// <param name="instanceId">The Instance ID to unblock.</param>
    void RemoveBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Empties the device instances list. Useful if <see cref="AddBlockedInstanceId" /> or
    ///     <see cref="BlockedInstanceIds" /> throw exceptions due to nonexistent entries.
    /// </summary>
    /// <remarks>
    ///     Be very conservative in using this call, you might accidentally undo settings different apps have put in
    ///     place.
    /// </remarks>
    void ClearBlockedInstancesList();

    /// <summary>
    ///     Submit a new application to allow (or deny if inverse flag is set).
    /// </summary>
    /// <remarks>Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").</remarks>
    /// <param name="path">The absolute application path to allow.</param>
    void AddApplicationPath(string path);

    /// <summary>
    ///     Revokes an applications exemption.
    /// </summary>
    /// <remarks>Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").</remarks>
    /// <param name="path">The absolute application path to revoke.</param>
    void RemoveApplicationPath(string path);

    /// <summary>
    ///     Empties the application list. Useful if <see cref="AddApplicationPath" /> or <see cref="ApplicationPaths" /> throw
    ///     exceptions due to nonexistent entries.
    /// </summary>
    /// <remarks>
    ///     Be very conservative in using this call, you might accidentally undo settings different apps have put in
    ///     place.
    /// </remarks>
    void ClearApplicationsList();
}
```

## 3rd party sources

- [C#/Win32 P/Invoke Source Generator](https://github.com/microsoft/CsWin32)
- [MinVer](https://github.com/adamralph/minver)
