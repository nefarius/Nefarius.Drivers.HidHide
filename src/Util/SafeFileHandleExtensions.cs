using System.Runtime.InteropServices;

using Windows.Win32.Foundation;

using Microsoft.Win32.SafeHandles;

using Nefarius.Drivers.HidHide.Exceptions;

namespace Nefarius.Drivers.HidHide.Util;

internal static class SafeFileHandleExtensions
{
    /// <summary>
    ///     Validates the state of a SafeFileHandle and throws an appropriate exception if the handle state is invalid.
    /// </summary>
    /// <param name="handle">The SafeFileHandle to validate.</param>
    /// <returns>The validated SafeFileHandle if it's in a valid state.</returns>
    /// <exception cref="HidHideDriverAccessFailedException">Thrown when access to the handle is denied.</exception>
    /// <exception cref="HidHideDriverNotFoundException">Thrown when the handle does not refer to a valid object.</exception>
    internal static SafeFileHandle HaltAndCatchFireOnError(this SafeFileHandle handle)
    {
        if (!handle.IsInvalid || handle.IsClosed)
        {
            return handle;
        }

        throw (WIN32_ERROR)Marshal.GetLastWin32Error() switch
        {
            WIN32_ERROR.ERROR_ACCESS_DENIED => new HidHideDriverAccessFailedException(),
            WIN32_ERROR.ERROR_NOT_FOUND => new HidHideDriverNotFoundException(),
            WIN32_ERROR.ERROR_INVALID_HANDLE => new HidHideHandleInvalidException(),
            _ => new HidHideWin32ErrorException()
        };
    }
}