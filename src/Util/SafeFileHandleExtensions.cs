using System.Runtime.InteropServices;

using Windows.Win32.Foundation;

using Microsoft.Win32.SafeHandles;

namespace Nefarius.Drivers.HidHide.Util;

internal static class SafeFileHandleExtensions
{
    /// <summary>
    ///     Throws an exception on invalid handle state.
    /// </summary>
    /// <param name="handle">The handle to test.</param>
    /// <exception cref="HidHideDriverAccessFailedException"></exception>
    /// <exception cref="HidHideDriverNotFoundException"></exception>
    internal static SafeFileHandle HaltAndCatchFireOnError(this SafeFileHandle handle)
    {
        if (!handle.IsInvalid || handle.IsClosed)
        {
            return handle;
        }

        switch ((WIN32_ERROR)Marshal.GetLastWin32Error())
        {
            case WIN32_ERROR.ERROR_ACCESS_DENIED:
                throw new HidHideDriverAccessFailedException();
            case WIN32_ERROR.ERROR_NOT_FOUND:
                throw new HidHideDriverNotFoundException();
        }
        
        return handle;
    }
}