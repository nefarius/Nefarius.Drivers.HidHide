using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;
using JetBrains.Annotations;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Describes a HidHide API exception.
/// </summary>
public abstract class HidHideException : Exception
{
    protected HidHideException()
    {
    }

    protected HidHideException(string message) : base(message)
    {
        NativeErrorCode = Marshal.GetLastWin32Error();
    }

    protected HidHideException(string message, int errorCode) : this(message)
    {
        NativeErrorCode = errorCode;
    }

    /// <summary>
    ///     Gets the native Win32 error code of the failed operation.
    /// </summary>
    [UsedImplicitly]
    public int NativeErrorCode { get; }

    /// <summary>
    ///     Gets the error message related to <see cref="NativeErrorCode" />.
    /// </summary>
    [UsedImplicitly]
    public unsafe string NativeErrorMessage
    {
        get
        {
            var buffer = stackalloc char[1024];

            var chars = PInvoke.FormatMessage(
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

public sealed class HidHideDriverAccessFailedException : HidHideException
{
    internal HidHideDriverAccessFailedException() : base(
        "Failed to open handle to driver. Make sure no other process is using the API at the same time.")
    {
    }
}

public sealed class HidHideDriverNotFoundException : HidHideException
{
    internal HidHideDriverNotFoundException() : base(
        "Failed to locate driver. Make sure HidHide is installed and not in a faulty state.")
    {
    }
}

public sealed class HidHideBufferOverflowException : HidHideException
{
    internal HidHideBufferOverflowException() : base(
        $"Buffer size exceeded maximum allowed value of {short.MaxValue} characters.")
    {
    }
}

public sealed class HidHideRequestFailedException : HidHideException
{
    internal HidHideRequestFailedException() : base(
        "Request failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
    }
}

public sealed class HidHideDetectionFailedException : HidHideException
{
    internal HidHideDetectionFailedException() : base(
        "Interface lookup failed. Check the 'NativeErrorCode' and 'NativeErrorMessage' property for more details.")
    {
    }
}