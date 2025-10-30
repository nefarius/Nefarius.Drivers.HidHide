using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.Devices.DeviceAndDriverInstallation;
using Windows.Win32.System.Diagnostics.Debug;

namespace Nefarius.Drivers.HidHide.Exceptions;

/// <summary>
///     Describes a HidHide API exception.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class HidHideException : Exception
{
    internal HidHideException()
    {
    }

    internal HidHideException(string message) : base(message)
    {
        NativeErrorCode = Marshal.GetLastWin32Error();
    }

    internal HidHideException(string message, int errorCode) : this(message)
    {
        NativeErrorCode = errorCode;
    }

    /// <summary>
    ///     Gets the native Win32 error code of the failed operation.
    /// </summary>
    public int NativeErrorCode { get; }

    /// <summary>
    ///     Gets the error message related to <see cref="NativeErrorCode" />.
    /// </summary>
    public unsafe string NativeErrorMessage
    {
        get
        {
            char* buffer = stackalloc char[1024];

            uint chars = PInvoke.FormatMessage(
                FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_FROM_SYSTEM |
                FORMAT_MESSAGE_OPTIONS.FORMAT_MESSAGE_IGNORE_INSERTS,
                null,
                (uint)NativeErrorCode,
                0,
                buffer,
                512,
                null
            );

            return chars > 0 ? new string(buffer).TrimEnd('\r', '\n') : null;
        }
    }

    /// <inheritdoc />
    public override string Message =>
        string.IsNullOrEmpty(NativeErrorMessage)
            ? base.Message
            : $"{base.Message}\r\nWin32 error: {NativeErrorMessage} ({NativeErrorCode})";
}

/// <summary>
///     Failed to open a handle to the driver. Make sure no other process is using the API at the same time.
/// </summary>
public sealed class HidHideDriverAccessFailedException : HidHideException
{
    internal HidHideDriverAccessFailedException() : base(
        "Failed to open handle to driver. Make sure no other process is using the API at the same time.")
    {
    }
}

/// <summary>
///     Indicates that the driver handle is invalid. Ensure that the driver is installed properly and operational.
/// </summary>
public sealed class HidHideHandleInvalidException : HidHideException
{
    internal HidHideHandleInvalidException() : base(
        "Failed to open handle to driver. Make sure the driver is installed properly and operational.")
    {
    }
}

/// <summary>
///     Represents an exception caused by an unknown Win32 error in the HidHide API.
///     Provides additional details through the 'NativeErrorCode' and 'NativeErrorMessage' properties.
/// </summary>
public sealed class HidHideWin32ErrorException : HidHideException
{
    internal HidHideWin32ErrorException() : base(
        "An unknown Win32 error occurred. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
    }
}

/// <summary>
///     Failed to locate the driver. Make sure HidHide is installed and not in a faulty state.
/// </summary>
public sealed class HidHideDriverNotFoundException : HidHideException
{
    internal HidHideDriverNotFoundException() : base(
        "Failed to locate driver. Make sure HidHide is installed and not in a faulty state.")
    {
    }
}

/// <summary>
///     More than one software device node was found. This can lead to unexpected behavior. Please uninstall the driver and reinstall it.
/// </summary>
public sealed class HidHideMultipleDeviceNodesFoundException : HidHideException
{
    internal HidHideMultipleDeviceNodesFoundException() : base(
        "More than one software device node was found. This can lead to unexpected behavior. Please uninstall the driver and reinstall it.")
    {
    }
}

/// <summary>
///     Buffer size exceeded the maximum allowed characters.
/// </summary>
public sealed class HidHideBufferOverflowException : HidHideException
{
    internal HidHideBufferOverflowException() : base(
        $"Buffer size exceeded maximum allowed value of {short.MaxValue} characters.")
    {
    }
}

/// <summary>
///     Request failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.
/// </summary>
public sealed class HidHideRequestFailedException : HidHideException
{
    internal HidHideRequestFailedException() : base(
        "Request failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
    }
}

/// <summary>
///     Interface lookup failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.
/// </summary>
public sealed class HidHideDetectionFailedException : HidHideException
{
    internal HidHideDetectionFailedException(CONFIGRET result) : base(
        "Interface lookup failed. Check the 'LastResult', 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
        LastResult = (int)result;
    }

    /// <summary>
    ///     The <see cref="CONFIGRET" /> of the failing call.
    /// </summary>
    public int LastResult { get; }
}