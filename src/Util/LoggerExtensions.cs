#nullable enable

using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace Nefarius.Drivers.HidHide.Util;

internal static class LoggerExtensions
{
    public static IDisposable? StartScope<T>(this ILogger<T>? logger)
    {
        return logger?.BeginScope(new Dictionary<string, object> { ["SourceContext"] = "Nefarius.Drivers.HidHide" });
    }
}