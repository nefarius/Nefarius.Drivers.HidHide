using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;

namespace Nefarius.Drivers.HidHide;

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
}

/// <summary>
///     Failed to open handle to driver. Make sure no other process is using the API at the same time.
/// </summary>
public sealed class HidHideDriverAccessFailedException : HidHideException
{
    internal HidHideDriverAccessFailedException() : base(
        "Failed to open handle to driver. Make sure no other process is using the API at the same time.")
    {
    }
}

/// <summary>
///     Failed to locate driver. Make sure HidHide is installed and not in a faulty state.
/// </summary>
public sealed class HidHideDriverNotFoundException : HidHideException
{
    internal HidHideDriverNotFoundException() : base(
        "Failed to locate driver. Make sure HidHide is installed and not in a faulty state.")
    {
    }
}

/// <summary>
///     Buffer size exceeded maximum allowed characters.
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
    internal HidHideDetectionFailedException() : base(
        "Interface lookup failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
    }
}