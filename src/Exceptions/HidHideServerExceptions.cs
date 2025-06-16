using System;
using System.Diagnostics.CodeAnalysis;

namespace Nefarius.Drivers.HidHide.Exceptions;

/// <summary>
///     Describes a HidHide CDN server exception.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public abstract class HidHideServerExceptions : Exception
{
    /// <inheritdoc />
    internal HidHideServerExceptions() { }

    internal HidHideServerExceptions(string message) : base(message)
    {
    }

    internal HidHideServerExceptions(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     Update response object missing, couldn't be deserialized or server error. Check
///     <see cref="Exception.InnerException" /> for details.
/// </summary>
public sealed class UpdateResponseMissingException : HidHideServerExceptions
{
    internal UpdateResponseMissingException(Exception innerException = null) : base(
        "Update response object missing, couldn't be deserialized or server error. Check InnerException for details.",
        innerException)
    {
    }
}

/// <summary>
///     Server didn't supply any release information.
/// </summary>
public sealed class MissingReleasesException : HidHideServerExceptions
{
    internal MissingReleasesException() : base("Server didn't supply any release information.") { }
}

/// <summary>
///     Download location URL wasn't set for the selected release.
/// </summary>
public sealed class DownloadLocationMissingException : HidHideServerExceptions
{
    internal DownloadLocationMissingException() : base("Download location URL wasn't set for the selected release.") { }
}

/// <summary>
///     The supplied URL was ill formatted.
/// </summary>
public sealed class MalformedUrlException : HidHideServerExceptions
{
    internal MalformedUrlException() : base("The supplied URL was ill formatted.") { }
}