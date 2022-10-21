using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Nefarius.Drivers.HidHide.Util;

namespace Nefarius.Drivers.HidHide;

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
    ///     Returns list of currently blocked instance IDs.
    /// </summary>
    IEnumerable<string> BlockedInstanceIds { get; }

    /// <summary>
    ///     Returns list of currently allowed application paths.
    /// </summary>
    IEnumerable<string> AllowedApplicationPaths { get; }

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
    ///     Submit a new application to allow (or deny if inverse flag is set).
    /// </summary>
    /// <param name="path">The absolute application path to allow.</param>
    void AddApplicationPath(string path);

    /// <summary>
    ///     Revokes an applications exemption.
    /// </summary>
    /// <param name="path">The absolute application path to revoke.</param>
    void RemoveApplicationPath(string path);
}

/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
public sealed class HidHideControlService : IHidHideControlService
{
    private const uint IoControlDeviceType = 32769;

    private const string ControlDeviceFilename = "\\\\.\\HidHide";

    private static readonly uint IoctlGetWhitelist =
        CTL_CODE(IoControlDeviceType, 2048, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlSetWhitelist =
        CTL_CODE(IoControlDeviceType, 2049, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlGetBlacklist =
        CTL_CODE(IoControlDeviceType, 2050, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlSetBlacklist =
        CTL_CODE(IoControlDeviceType, 2051, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlGetActive =
        CTL_CODE(IoControlDeviceType, 2052, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlSetActive =
        CTL_CODE(IoControlDeviceType, 2053, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlGetWlInverse =
        CTL_CODE(IoControlDeviceType, 2054, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    private static readonly uint IoctlSetWlInverse =
        CTL_CODE(IoControlDeviceType, 2055, PInvoke.METHOD_BUFFERED, PInvoke.FILE_READ_ACCESS);

    /// <inheritdoc />
    public unsafe bool IsActive
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var bufferLength = Marshal.SizeOf<bool>();
            var buffer = stackalloc byte[bufferLength];

            PInvoke.DeviceIoControl(
                handle,
                IoctlGetActive,
                buffer,
                (uint)bufferLength,
                buffer,
                (uint)bufferLength,
                null,
                null
            );

            return buffer[0] > 0;
        }
        set
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var bufferLength = Marshal.SizeOf<bool>();
            var buffer = stackalloc byte[bufferLength];

            buffer[0] = value ? (byte)1 : (byte)0;

            PInvoke.DeviceIoControl(
                handle,
                IoctlSetActive,
                buffer,
                (uint)bufferLength,
                null,
                0,
                null,
                null
            );
        }
    }

    /// <inheritdoc />
    public unsafe IEnumerable<string> BlockedInstanceIds
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var buffer = IntPtr.Zero;

            try
            {
                uint required = 0;

                // Get required buffer size
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetBlacklist,
                    null,
                    0,
                    null,
                    0,
                    &required,
                    null
                );

                buffer = Marshal.AllocHGlobal((int)required);

                // Get actual buffer content
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetBlacklist,
                    null,
                    0,
                    buffer.ToPointer(),
                    required,
                    null,
                    null
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray((int)required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe IEnumerable<string> AllowedApplicationPaths
    {
        get
        {
            using var handle = PInvoke.CreateFile(
                ControlDeviceFilename,
                FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
                FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
                null,
                FILE_CREATION_DISPOSITION.OPEN_EXISTING,
                FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
                null
            );

            var buffer = IntPtr.Zero;

            try
            {
                uint required = 0;

                // Get required buffer size
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetWhitelist,
                    null,
                    0,
                    null,
                    0,
                    &required,
                    null
                );

                buffer = Marshal.AllocHGlobal((int)required);

                // Get actual buffer content
                // Check return value for success
                PInvoke.DeviceIoControl(
                    handle,
                    IoctlGetWhitelist,
                    null,
                    0,
                    buffer.ToPointer(),
                    required,
                    null,
                    null
                );

                // Store existing block-list in a more manageable "C#" fashion
                return buffer.MultiSzPointerToStringArray((int)required).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void AddBlockedInstanceId(string instanceId)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = BlockedInstanceIds
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    instanceId
                })
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveBlockedInstanceId(string instanceId)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = BlockedInstanceIds
                .Where(i => !i.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void AddApplicationPath(string path)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = AllowedApplicationPaths
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    VolumeHelper.PathToDosDevicePath(path)
                })
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveApplicationPath(string path)
    {
        using var handle = PInvoke.CreateFile(
            ControlDeviceFilename,
            FILE_ACCESS_FLAGS.FILE_GENERIC_READ | FILE_ACCESS_FLAGS.FILE_GENERIC_WRITE,
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );

        var buffer = IntPtr.Zero;

        try
        {
            buffer = AllowedApplicationPaths
                .Where(i => !i.Equals(VolumeHelper.PathToDosDevicePath(path), StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out var length); // Convert to usable buffer

            // Submit new list
            // Check return value for success
            PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static UInt32 CTL_CODE(uint deviceType, uint function, uint method, uint access)
    {
        return (deviceType << 16) | (access << 14) | (function << 2) | method;
    }
}