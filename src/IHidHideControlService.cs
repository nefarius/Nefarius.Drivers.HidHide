using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Nefarius.Drivers.HidHide.Exceptions;
using Nefarius.Utilities.DeviceManagement.Exceptions;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public interface IHidHideControlService
{
    /// <summary>
    ///     Gets or sets whether global device hiding is currently active or not.
    /// </summary>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    bool IsActive { get; set; }

    /// <summary>
    ///     Gets whether the driver is present and operable.
    /// </summary>
    /// <exception cref="HidHideDetectionFailedException">
    ///     Driver lookup has failed. See <see cref="HidHideException.NativeErrorCode" /> and
    ///     <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    bool IsInstalled { get; }

    /// <summary>
    ///     Gets whether the virtual root-enumerated software device the driver attaches to is present on the system.
    /// </summary>
    /// <exception cref="ConfigManagerException">An unexpected enumeration error occurred.</exception>
    bool IsDriverNodePresent { get; }

    /// <summary>
    ///     Gets whether the driver node is present and operational (has its device interface exposed).
    /// </summary>
    /// <exception cref="HidHideDetectionFailedException">
    ///     Driver lookup has failed. See <see cref="HidHideException.NativeErrorCode" /> and
    ///     <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="ConfigManagerException">An unexpected enumeration error occurred.</exception>
    bool IsOperational { get; }

    /// <summary>
    ///     Gets the local driver binary version.
    /// </summary>
    /// <exception cref="ConfigManagerException">An unexpected enumeration error occurred.</exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideMultipleDeviceNodesFoundException">
    ///     More than one software device node was found. This can lead
    ///     to unexpected behavior. Please uninstall the driver and reinstall it.
    /// </exception>
    Version LocalDriverVersion { get; }

    /// <summary>
    ///     Gets or sets whether the application list is inverted (from block all/allow specific to allow all/block specific).
    /// </summary>
    /// <remarks>
    ///     The default behavior of the application list is to block all processes by default and only treat listed paths
    ///     as exempted.
    /// </remarks>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    bool IsAppListInverted { get; set; }

    /// <summary>
    ///     Returns a list of currently blocked instance IDs.
    /// </summary>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    IReadOnlyList<string> BlockedInstanceIds { get; }

    /// <summary>
    ///     Returns a list of currently allowed (or blocked, see <see cref="IsAppListInverted" />) application paths.
    /// </summary>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    IReadOnlyList<string> ApplicationPaths { get; }

    /// <summary>
    ///     Submit a new instance for blocking.
    /// </summary>
    /// <remarks>
    ///     To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
    ///     https://github.com/nefarius/Nefarius.Utilities.DeviceManagement
    /// </remarks>
    /// <param name="instanceId">The Instance ID to block.</param>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    void AddBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Remove an instance from being blocked.
    /// </summary>
    /// <remarks>
    ///     To get the instance ID from e.g. a symbolic link (device path) you can use this companion library:
    ///     https://github.com/nefarius/Nefarius.Utilities.DeviceManagement
    /// </remarks>
    /// <param name="instanceId">The Instance ID to unblock.</param>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    void RemoveBlockedInstanceId(string instanceId);

    /// <summary>
    ///     Empties the device instances list. Useful if <see cref="AddBlockedInstanceId" /> or
    ///     <see cref="BlockedInstanceIds" /> throw exceptions due to nonexistent entries.
    /// </summary>
    /// <remarks>
    ///     Be very conservative in using this call, you might accidentally undo settings different apps have put in
    ///     place.
    /// </remarks>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    void ClearBlockedInstancesList();

    /// <summary>
    ///     Submit a new application to allow (or deny if the inverse flag is set).
    /// </summary>
    /// <remarks>Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").</remarks>
    /// <param name="path">The absolute application path to allow.</param>
    /// <param name="throwIfInvalid">Throws exception if an invalid (nonexistent) file path is supplied.</param>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver.
    ///     Make sure no other process is using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver.
    ///     Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed.
    ///     See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters.
    ///     It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///     <paramref name="throwIfInvalid" /> was set and the supplied file
    ///     <paramref name="path" /> wasn't found.
    /// </exception>
    void AddApplicationPath(string path, bool throwIfInvalid = true);

    /// <summary>
    ///     Revokes an applications exemption.
    /// </summary>
    /// <remarks>Use the common local path notation (e.g. "C:\Windows\System32\rundll32.exe").</remarks>
    /// <param name="path">The absolute application path to revoke.</param>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver. Make sure no other process is
    ///     using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    void RemoveApplicationPath(string path);

    /// <summary>
    ///     Empties the application list. Useful if <see cref="AddApplicationPath" /> or <see cref="ApplicationPaths" /> throw
    ///     exceptions due to nonexistent entries.
    /// </summary>
    /// <remarks>
    ///     Be very conservative in using this call, you might accidentally undo settings different apps have put in
    ///     place.
    /// </remarks>
    /// <exception cref="HidHideDriverAccessFailedException">
    ///     Failed to open a handle to the driver. Make sure no other process is
    ///     using the API at the same time.
    /// </exception>
    /// <exception cref="HidHideDriverNotFoundException">
    ///     Failed to locate the driver. Make sure HidHide is installed and not in a
    ///     faulty state.
    /// </exception>
    /// <exception cref="HidHideRequestFailedException">
    ///     Driver communication has failed. See
    ///     <see cref="HidHideException.NativeErrorCode" /> and <see cref="HidHideException.NativeErrorMessage" /> for details.
    /// </exception>
    /// <exception cref="HidHideBufferOverflowException">
    ///     Buffer size exceeded the maximum allowed characters. It happens when the list
    ///     grew out of supported bounds.
    /// </exception>
    void ClearApplicationsList();
}