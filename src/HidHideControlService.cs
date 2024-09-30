#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;

using Nefarius.Drivers.HidHide.Exceptions;
using Nefarius.Drivers.HidHide.Util;
using Nefarius.Utilities.DeviceManagement.Extensions;
using Nefarius.Utilities.DeviceManagement.PnP;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Provides a managed wrapper for communicating with HidHide driver.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public sealed class HidHideControlService : IHidHideControlService
{
    private const uint IoControlDeviceType = 32769;

    private const string ControlDeviceFilename = "\\\\.\\HidHide";

    private static readonly uint IoctlGetWhitelist =
        CTL_CODE(IoControlDeviceType, 2048, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlSetWhitelist =
        CTL_CODE(IoControlDeviceType, 2049, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlGetBlacklist =
        CTL_CODE(IoControlDeviceType, 2050, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlSetBlacklist =
        CTL_CODE(IoControlDeviceType, 2051, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlGetActive =
        CTL_CODE(IoControlDeviceType, 2052, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlSetActive =
        CTL_CODE(IoControlDeviceType, 2053, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlGetWlInverse =
        CTL_CODE(IoControlDeviceType, 2054, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private static readonly uint IoctlSetWlInverse =
        CTL_CODE(IoControlDeviceType, 2055, PInvoke.METHOD_BUFFERED, FILE_ACCESS_RIGHTS.FILE_READ_DATA);

    private readonly ILogger<HidHideControlService>? _logger;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    ///     Creates a new instance of <see cref="HidHideControlService" /> that is DI-aware.
    /// </summary>
    /// <param name="loggerFactory">Injects a logging factory.</param>
    public HidHideControlService(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<HidHideControlService>();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="HidHideControlService" /> that is not DI-aware.
    /// </summary>
    /// <remarks>
    ///     If the caller uses a dependency injection framework, do not instantiate this class directly. Use
    ///     <see cref="ServiceCollectionExtensions.AddHidHide" /> instead.
    /// </remarks>
    public HidHideControlService() { }

    /// <summary>
    ///     Interface GUID to enumerate HidHide devices.
    /// </summary>
    public static Guid DeviceInterface => Guid.Parse("{0C320FF7-BD9B-42B6-BDAF-49FEB9C91649}");

    /// <summary>
    ///     Hardware ID of the root-enumerated software node the driver attaches to.
    /// </summary>
    public static string HardwareId => @"Root\HidHide";

    /// <inheritdoc />
    public unsafe bool IsActive
    {
        get
        {
            _logger?.LogDebug("Fetching active status");

            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            int bufferLength = Marshal.SizeOf<byte>();
            byte* buffer = stackalloc byte[bufferLength];

            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetActive,
                null,
                0,
                buffer,
                (uint)bufferLength,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            _logger?.LogDebug("Got active status {IsActive}", buffer[0]);

            return buffer[0] > 0;
        }
        set
        {
            _logger?.LogDebug("Setting active status to {IsActive}", value);

            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            int bufferLength = Marshal.SizeOf<byte>();
            byte* buffer = stackalloc byte[bufferLength];

            buffer[0] = value ? (byte)1 : (byte)0;

            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetActive,
                buffer,
                (uint)bufferLength,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
    }

    /// <inheritdoc />
    public unsafe bool IsInstalled
    {
        get
        {
            _logger?.LogDebug("Fetching installed status");

            // query for required buffer size (in characters)
            CONFIGRET ret = PInvoke.CM_Get_Device_Interface_List_Size(
                out uint length,
                DeviceInterface,
                null,
                PInvoke.CM_GET_DEVICE_INTERFACE_LIST_PRESENT
            );

            if (ret != CONFIGRET.CR_SUCCESS)
            {
                throw new HidHideDetectionFailedException(ret);
            }

            reAlloc:
            // allocate required bytes (wide characters)
            IntPtr buffer = Marshal.AllocHGlobal((int)length * 2);
            Unsafe.InitBlockUnaligned(buffer.ToPointer(), 0, length * 2);

            try
            {
                // grab the actual buffer
                ret = PInvoke.CM_Get_Device_Interface_List(
                    DeviceInterface,
                    null,
                    new PZZWSTR((char*)buffer.ToPointer()),
                    length,
                    PInvoke.CM_GET_DEVICE_INTERFACE_LIST_PRESENT
                );

                // see: https://learn.microsoft.com/en-us/windows/win32/api/cfgmgr32/nf-cfgmgr32-cm_get_device_interface_lista#remarks
                if (ret == CONFIGRET.CR_BUFFER_SMALL)
                {
                    // prepare for trouble...
                    Marshal.FreeHGlobal(buffer);
                    // ...and make it double!
                    length *= 2;
                    goto reAlloc;
                }

                if (ret != CONFIGRET.CR_SUCCESS)
                {
                    throw new HidHideDetectionFailedException(ret);
                }

                // convert to managed string
                string firstInstanceId = new((char*)buffer.ToPointer());

                // if HidHide is not loaded, the returned list will be empty
                bool isInstalled = !string.IsNullOrEmpty(firstInstanceId);

                _logger?.LogDebug("Got installed status {IsInstalled}", isInstalled);

                return isInstalled;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
    }

    /// <inheritdoc />
    public bool IsDriverNodePresent =>
        Devcon.FindInDeviceClassByHardwareId(
            DeviceClassIds.System, HardwareId);

    /// <inheritdoc />
    public bool IsOperational => IsInstalled && IsDriverNodePresent;

    /// <inheritdoc />
    public Version LocalDriverVersion
    {
        get
        {
            if (!Devcon.FindInDeviceClassByHardwareId(
                    DeviceClassIds.System, HardwareId,
                    out IEnumerable<string> hhInstances)
               )
            {
                throw new HidHideDriverNotFoundException();
            }

            PnPDevice virtualDevice = PnPDevice.GetDeviceByInstanceId(hhInstances.Single());
            return virtualDevice.GetCurrentDriver().DriverVersion;
        }
    }

    /// <inheritdoc />
    public unsafe bool IsAppListInverted
    {
        get
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            int bufferLength = Marshal.SizeOf<byte>();
            byte* buffer = stackalloc byte[bufferLength];

            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetWlInverse,
                null,
                0,
                buffer,
                (uint)bufferLength,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            return buffer[0] > 0;
        }
        set
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            int bufferLength = Marshal.SizeOf<byte>();
            byte* buffer = stackalloc byte[bufferLength];

            buffer[0] = value ? (byte)1 : (byte)0;

            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetWlInverse,
                buffer,
                (uint)bufferLength,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> BlockedInstanceIds
    {
        get
        {
            using IDisposable? scope = _logger?.StartScope();

            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            return GetBlockedInstances(handle);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ApplicationPaths
    {
        get
        {
            using IDisposable? scope = _logger?.StartScope();

            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            return GetApplications(handle);
        }
    }

    /// <inheritdoc />
    public unsafe void AddBlockedInstanceId(string instanceId)
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Adding instance: {Instance}", instanceId);

        IntPtr buffer = IntPtr.Zero;

        try
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            buffer = GetBlockedInstances(handle)
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    instanceId
                })
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            if (length >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveBlockedInstanceId(string instanceId)
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Removing instance: {Instance}", instanceId);

        IntPtr buffer = IntPtr.Zero;

        try
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            buffer = GetBlockedInstances(handle)
                .Where(i => !i.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            if (length >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void ClearBlockedInstancesList()
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Clearing blocked instances list");

        IntPtr buffer = IntPtr.Zero;

        try
        {
            buffer = Array.Empty<string>().StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetBlacklist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void AddApplicationPath(string path)
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Adding application: {Path}", path);

        IntPtr buffer = IntPtr.Zero;

        try
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            buffer = GetApplications(handle)
                .Concat(new[] // Add our own instance paths to the existing list
                {
                    path
                })
                .Distinct() // Remove duplicates, if any
                .Select(p =>
                    new VolumeHelper(_loggerFactory?.CreateLogger<VolumeHelper>())
                        .PathToDosDevicePath(p, false)) // re-convert to dos paths
                .Where(r => !string.IsNullOrEmpty(r)) // strip invalid entries
                .StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            if (length >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void RemoveApplicationPath(string path)
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Removing application: {Path}", path);

        IntPtr buffer = IntPtr.Zero;

        try
        {
            using SafeFileHandle handle = OpenControlDeviceHandle().HaltAndCatchFireOnError();

            buffer = GetApplications(handle)
                .Where(i => !i.Equals(path, StringComparison.OrdinalIgnoreCase))
                .Distinct() // Remove duplicates, if any
                .Select(p =>
                    new VolumeHelper(_loggerFactory?.CreateLogger<VolumeHelper>())
                        .PathToDosDevicePath(p, false)) // re-convert to dos paths
                .Where(r => !string.IsNullOrEmpty(r)) // strip invalid entries
                .StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            if (length >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    /// <inheritdoc />
    public unsafe void ClearApplicationsList()
    {
        using IDisposable? scope = _logger?.StartScope();

        _logger?.LogDebug("Clearing applications list");

        IntPtr buffer = IntPtr.Zero;

        try
        {
            buffer = Array.Empty<string>().StringArrayToMultiSzPointer(out int length); // Convert to usable buffer

            using SafeFileHandle handle = OpenControlDeviceHandle();

            handle.HaltAndCatchFireOnError();

            // Submit new list
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlSetWhitelist,
                buffer.ToPointer(),
                (uint)length,
                null,
                0,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    private static SafeFileHandle OpenControlDeviceHandle()
    {
        return PInvoke.CreateFile(
            ControlDeviceFilename,
            (uint)(FILE_ACCESS_RIGHTS.FILE_GENERIC_READ | FILE_ACCESS_RIGHTS.FILE_GENERIC_WRITE),
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL,
            null
        );
    }

    private unsafe IReadOnlyList<string> GetApplications(SafeHandle handle)
    {
        IntPtr buffer = IntPtr.Zero;

        try
        {
            uint required = 0;

            // Get required buffer size
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetWhitelist,
                null,
                0,
                null,
                0,
                &required,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            if (required >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            buffer = Marshal.AllocHGlobal((int)required);

            // Get actual buffer content
            // Check return value for success
            ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetWhitelist,
                null,
                0,
                buffer.ToPointer(),
                required,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            // Store existing block-list in a more manageable "C#" fashion
            List<string?> list = buffer
                .MultiSzPointerToStringArray((int)required)
                .Select(p =>
                    new VolumeHelper(_loggerFactory?.CreateLogger<VolumeHelper>()).DosDevicePathToPath(p, false))
                .Where(r => !string.IsNullOrEmpty(r))
                .ToList();

            _logger?.LogDebug("Got applications: {@AppList}", list);

            return list!;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    private unsafe IReadOnlyList<string> GetBlockedInstances(SafeHandle handle)
    {
        IntPtr buffer = IntPtr.Zero;

        try
        {
            uint required = 0;

            // Get required buffer size
            BOOL ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetBlacklist,
                null,
                0,
                null,
                0,
                &required,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            if (required >= short.MaxValue)
            {
                throw new HidHideBufferOverflowException();
            }

            buffer = Marshal.AllocHGlobal((int)required);

            // Get actual buffer content
            ret = PInvoke.DeviceIoControl(
                handle,
                IoctlGetBlacklist,
                null,
                0,
                buffer.ToPointer(),
                required,
                null,
                null
            );

            if (!ret)
            {
                throw new HidHideRequestFailedException();
            }

            // Store existing block-list in a more manageable "C#" fashion
            List<string> list = buffer.MultiSzPointerToStringArray((int)required).ToList();

            _logger?.LogDebug("Got instances: {@AppList}", list);

            return list;
        }
        finally
        {
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }

    private static UInt32 CTL_CODE(uint deviceType, uint function, uint method, FILE_ACCESS_RIGHTS access)
    {
        return (deviceType << 16) | ((uint)access << 14) | (function << 2) | method;
    }
}