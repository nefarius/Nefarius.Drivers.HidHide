using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.System.Diagnostics.Debug;
using JetBrains.Annotations;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Describes a HidHide API exception.
/// </summary>
public class HidHideException : Exception
{
    public HidHideException()
    {
    }

    public HidHideException(string message) : base(message)
    {
        NativeErrorCode = Marshal.GetLastWin32Error();
    }

    public HidHideException(string message, int errorCode) : this(message)
    {
        NativeErrorCode = errorCode;
    }

    /// <summary>
    ///     Gets the native Win32 error code of the failed operation.
    /// </summary>
    [UsedImplicitly]
    public int NativeErrorCode { get; }

    /// <summary>
    ///     Gets the error message related to <see cref="NativeErrorCode"/>.
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

            return chars > 0 ? new string(buffer) : null;
        }
    }
}