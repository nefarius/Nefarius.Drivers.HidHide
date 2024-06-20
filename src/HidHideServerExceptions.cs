using System;

using Nefarius.Vicius.Abstractions.Models;

namespace Nefarius.Drivers.HidHide;

/// <summary>
///     Describes a HidHide CDN server exception.
/// </summary>
public abstract class HidHideServerExceptions : Exception
{
    internal HidHideServerExceptions() { }

    internal HidHideServerExceptions(string message) : base(message)
    {
    }

    internal HidHideServerExceptions(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
///     A valid <see cref="UpdateResponse" /> wasn't returned.
/// </summary>
public sealed class UpdateResponseEmptyException : HidHideServerExceptions
{
    internal UpdateResponseEmptyException() : base("Update response object missing or couldn't be deserialized.") { }
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