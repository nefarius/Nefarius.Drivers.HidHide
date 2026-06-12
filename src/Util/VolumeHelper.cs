#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using Windows.Win32;
using Windows.Win32.Foundation;

using Microsoft.Extensions.Logging;

namespace Nefarius.Drivers.HidHide.Util;

/// <summary>
///     Path manipulation and volume helper methods.
/// </summary>
internal class VolumeHelper
{
    private readonly ILogger<VolumeHelper>? _logger;
    private readonly IVolumeMappingProvider _volumeMappingProvider;
    private IReadOnlyList<VolumeMapping>? _mappings;

    internal VolumeHelper(ILogger<VolumeHelper>? logger)
        : this(logger, new WindowsVolumeMappingProvider())
    {
    }

    internal VolumeHelper(ILogger<VolumeHelper>? logger, IVolumeMappingProvider volumeMappingProvider)
    {
        _logger = logger;
        _volumeMappingProvider = volumeMappingProvider;
    }

    private IReadOnlyList<VolumeMapping> GetMappings()
        => _mappings ??= _volumeMappingProvider.GetVolumeMappings();

    /// <summary>
    ///     Splits Win32 MULTI_SZ buffer content into individual entries.
    /// </summary>
    /// <param name="value">The MULTI_SZ string.</param>
    /// <returns>The non-empty entries.</returns>
    internal static IReadOnlyList<string> ParseMultiString(string value)
    {
        return value
            .Split('\0')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    /// <summary>
    ///     Canonicalizes a path so matching and relative-path extraction operate on the same string.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The fully qualified path, or null if the path cannot be resolved.</returns>
    private static string? TryGetFullPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException
                                       or SecurityException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Translates a "DOS device" path to user-land path.
    /// </summary>
    /// <param name="devicePath">The DOS device path to convert.</param>
    /// <param name="throwOnError">
    ///     When true, throws <see cref="ArgumentException" /> if no matching mount point is found.
    ///     When false, returns null instead. Note: <paramref name="throwOnError" /> only governs path
    ///     translation failures. A <see cref="System.ComponentModel.Win32Exception" /> raised by the
    ///     underlying volume enumeration always propagates regardless of this flag.
    /// </param>
    /// <returns>The user-land path.</returns>
    public string? DosDevicePathToPath(string devicePath, bool throwOnError = true)
    {
        _logger?.LogDebug("++ Resolving DOS device path {DevicePath} to path", devicePath);

        VolumeMapping? bestMapping = null;
        string? bestDevicePath = null;

        foreach (VolumeMapping current in GetMappings())
        {
            if (string.IsNullOrWhiteSpace(current.MountPoint) || string.IsNullOrWhiteSpace(current.DevicePath))
            {
                continue;
            }

            string currentDevicePath = current.DevicePath.TrimEnd(Path.DirectorySeparatorChar);

            if (currentDevicePath.Length == 0)
            {
                continue;
            }

            // Boundary-checked prefix match so e.g. HarddiskVolume1 never claims HarddiskVolume10 paths
            bool isMatch =
                devicePath.Equals(currentDevicePath, StringComparison.OrdinalIgnoreCase) ||
                devicePath.StartsWith(currentDevicePath + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            // Prefer the longer device path; on equal length prefer the longer mount point so that
            // display is deterministic when one volume is mounted at multiple locations.
            bool isBetter = bestDevicePath is null ||
                            currentDevicePath.Length > bestDevicePath.Length ||
                            (currentDevicePath.Length == bestDevicePath.Length &&
                             current.MountPoint.Length > bestMapping!.MountPoint.Length);

            if (isBetter)
            {
                bestMapping = current;
                bestDevicePath = currentDevicePath;
            }
        }

        if (bestMapping is null || bestDevicePath is null)
        {
            _logger?.LogDebug("No volume mapping matched device path {DevicePath}", devicePath);

            if (throwOnError)
            {
                throw new ArgumentException("Failed to translate provided path");
            }

            return null;
        }

        string relativePath = devicePath.Length > bestDevicePath.Length
            ? devicePath.Substring(bestDevicePath.Length).TrimStart(Path.DirectorySeparatorChar)
            : string.Empty;

        _logger?.LogDebug("Built relative path: {Path}", relativePath);

        // Concatenate instead of Path.Combine so a remainder that merely looks rooted
        // (e.g. "C:evil.exe" from a corrupt blocklist entry) can't discard the mount point
        string fullPath =
            bestMapping.MountPoint.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar +
            relativePath;

        _logger?.LogDebug("-- Returning path {Path} for device path {DevicePath}", fullPath, devicePath);

        return fullPath;
    }

    /// <summary>
    ///     Translates a user-land file path to "DOS device" path.
    /// </summary>
    /// <param name="path">The file path in normal namespace format.</param>
    /// <param name="throwOnError">
    ///     When true, throws on parse/translation failures (file not found, no matching mount point).
    ///     When false, returns null instead. Note: <paramref name="throwOnError" /> only governs path
    ///     translation failures. A <see cref="System.ComponentModel.Win32Exception" /> raised by the
    ///     underlying volume enumeration always propagates regardless of this flag.
    /// </param>
    /// <returns>The device namespace path (DOS device).</returns>
    public string? PathToDosDevicePath(string path, bool throwOnError = true)
    {
        _logger?.LogDebug("++ Resolving path {Path} to DOS device path", path);

        //
        // TODO: cover and test junctions!
        //

        if (!File.Exists(path))
        {
            _logger?.LogWarning("The provided path {Path} doesn't exist", path);

            if (throwOnError)
            {
                throw new ArgumentException($"The supplied file path {path} doesn't exist", nameof(path));
            }

            return null;
        }

        string? fullPath = TryGetFullPath(path);

        if (fullPath is null)
        {
            _logger?.LogWarning("Failed to canonicalize path {Path}", path);

            if (throwOnError)
            {
                throw new IOException($"Couldn't canonicalize path {path}");
            }

            return null;
        }

        VolumeMapping? bestMapping = null;
        string? bestMountPoint = null;

        foreach (VolumeMapping current in GetMappings())
        {
            if (string.IsNullOrWhiteSpace(current.MountPoint) || string.IsNullOrWhiteSpace(current.DevicePath))
            {
                continue;
            }

            string? mountPointFullPath = TryGetFullPath(current.MountPoint);

            if (mountPointFullPath is null)
            {
                _logger?.LogDebug("Skipping unusable mount point {MountPoint}", current.MountPoint);
                continue;
            }

            string mountPoint = mountPointFullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (mountPoint.Length == 0)
            {
                continue;
            }

            bool isMatch =
                fullPath.Equals(mountPoint, StringComparison.OrdinalIgnoreCase) ||
                fullPath.StartsWith(mountPoint + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            // Prefer the longest (deepest) mount point so mounted folders win over the volume root
            if (bestMountPoint is null || mountPoint.Length > bestMountPoint.Length)
            {
                bestMapping = current;
                bestMountPoint = mountPoint;
            }
        }

        if (bestMapping is null || bestMountPoint is null)
        {
            _logger?.LogWarning("Failed to resolve path {Path}", path);

            if (throwOnError)
            {
                throw new IOException($"Couldn't resolve device path of path {path}");
            }

            return null;
        }

        string relativePath = fullPath.Length > bestMountPoint.Length
            ? fullPath.Substring(bestMountPoint.Length).TrimStart(Path.DirectorySeparatorChar)
            : string.Empty;

        _logger?.LogDebug("Built relative path: {Path}", relativePath);

        StringBuilder fullDevicePath = new();
        fullDevicePath.Append(bestMapping.DevicePath.TrimEnd(Path.DirectorySeparatorChar));
        fullDevicePath.Append(Path.DirectorySeparatorChar);
        fullDevicePath.Append(relativePath);

        _logger?.LogDebug("-- Returning device path {DevicePath} for path {Path}", fullDevicePath, path);

        return fullDevicePath.ToString();
    }
}

/// <summary>
///     Supplies volume to mount point mappings.
/// </summary>
internal interface IVolumeMappingProvider
{
    /// <summary>
    ///     Gets one mapping per volume mount point; volumes without mount points are not included.
    /// </summary>
    /// <exception cref="Win32Exception">Volume enumeration failed.</exception>
    IReadOnlyList<VolumeMapping> GetVolumeMappings();
}

/// <summary>
///     Describes a single volume mount point and its NT device path.
/// </summary>
internal sealed class VolumeMapping
{
    public VolumeMapping(string mountPoint, string volumeName, string devicePath)
    {
        MountPoint = mountPoint;
        VolumeName = volumeName;
        DevicePath = devicePath;
    }

    /// <summary>
    ///     The user-land mount point (e.g. "C:\" or "C:\Mount\Data\").
    /// </summary>
    public string MountPoint { get; }

    /// <summary>
    ///     The volume GUID path (e.g. "\\?\Volume{...}\"), or an empty string when the mapping was
    ///     discovered from a DOS device definition instead of the Mount Manager.
    /// </summary>
    public string VolumeName { get; }

    /// <summary>
    ///     The NT device path (e.g. "\Device\HarddiskVolume2").
    /// </summary>
    public string DevicePath { get; }
}

/// <summary>
///     Queries volume mappings from the running Windows system.
/// </summary>
internal sealed class WindowsVolumeMappingProvider : IVolumeMappingProvider
{
    private static readonly HANDLE InvalidHandleValue = new(new IntPtr(-1));

    /// <inheritdoc />
    public unsafe IReadOnlyList<VolumeMapping> GetVolumeMappings()
    {
        char[] volumeNameBuffer = new char[ushort.MaxValue];
        char[] devicePathBuffer = new char[ushort.MaxValue];
        char[] mountPointsBuffer = new char[ushort.MaxValue];

        fixed (char* pVolumeName = volumeNameBuffer)
        fixed (char* pDevicePath = devicePathBuffer)
        fixed (char* pMountPoints = mountPointsBuffer)
        {
            List<VolumeMapping> list = new();

            HANDLE volumeHandle = PInvoke.FindFirstVolume(pVolumeName, ushort.MaxValue);

            if (volumeHandle == InvalidHandleValue)
            {
                // Distinguish "enumeration failed" from "no volumes": silently returning an empty
                // list would make callers rewrite the driver blocklist with nothing in it
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                do
                {
                    string volume = GetNullTerminatedString(volumeNameBuffer);

                    // Volume GUID paths are documented ("Naming a Volume") as \\?\Volume{...}\ with a
                    // trailing backslash; like the Win32 "Displaying Volume Paths" sample, treat any
                    // other shape as invalid, but skip it instead of aborting the enumeration.
                    if (volume.Length < 6 ||
                        !volume.StartsWith(@"\\?\", StringComparison.Ordinal) ||
                        volume[volume.Length - 1] != '\\')
                    {
                        continue;
                    }

                    Array.Clear(mountPointsBuffer, 0, mountPointsBuffer.Length);
                    Array.Clear(devicePathBuffer, 0, devicePathBuffer.Length);

                    if (!PInvoke.GetVolumePathNamesForVolumeName(
                            volume,
                            pMountPoints,
                            ushort.MaxValue,
                            out uint mountPointsLength
                        ))
                    {
                        continue;
                    }

                    int usableMountPointsLength = (int)Math.Min(mountPointsLength, (uint)mountPointsBuffer.Length);

                    IReadOnlyList<string> mountPoints = VolumeHelper.ParseMultiString(
                        new string(mountPointsBuffer, 0, usableMountPointsLength));

                    if (mountPoints.Count == 0)
                    {
                        // A volume without mount points can't participate in path translation
                        continue;
                    }

                    // Extract the volume name for use with QueryDosDevice (strip "\\?\" prefix and trailing separator)
                    string deviceName = volume.Substring(4, volume.Length - 1 - 4);

                    uint devicePathLength;

                    fixed (char* pDeviceName = deviceName)
                    {
                        devicePathLength = PInvoke.QueryDosDevice(pDeviceName, pDevicePath, ushort.MaxValue);
                    }

                    if (devicePathLength == 0)
                    {
                        continue;
                    }

                    int usableDevicePathLength = (int)Math.Min(devicePathLength, (uint)devicePathBuffer.Length);

                    // QueryDosDevice returns a MULTI_SZ list; the first entry is the current mapping
                    string? devicePath = VolumeHelper.ParseMultiString(
                            new string(devicePathBuffer, 0, usableDevicePathLength))
                        .FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(devicePath))
                    {
                        continue;
                    }

                    foreach (string mountPoint in mountPoints)
                    {
                        list.Add(new VolumeMapping(mountPoint, volume, devicePath!));
                    }
                } while (PInvoke.FindNextVolume(volumeHandle, pVolumeName, ushort.MaxValue));

                // FALSE with ERROR_NO_MORE_FILES is the documented normal end of enumeration;
                // anything else means the list is silently incomplete, so surface it
                int lastError = Marshal.GetLastWin32Error();

                if (lastError != (int)WIN32_ERROR.ERROR_NO_MORE_FILES)
                {
                    throw new Win32Exception(lastError);
                }
            }
            finally
            {
                PInvoke.FindVolumeClose(volumeHandle);
            }

            AppendDosDeviceDriveMappings(list, devicePathBuffer, pDevicePath);

            return list;
        }
    }

    /// <summary>
    ///     Adds mappings for drive letters defined outside the Mount Manager (DefineDosDevice-based
    ///     drives such as VeraCrypt, ImDisk or SUBST) which FindFirstVolume cannot enumerate.
    /// </summary>
    private static unsafe void AppendDosDeviceDriveMappings(List<VolumeMapping> list, char[] devicePathBuffer,
        char* pDevicePath)
    {
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            string mountPoint = letter + @":\";

            if (list.Any(m => m.MountPoint.Equals(mountPoint, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            Array.Clear(devicePathBuffer, 0, devicePathBuffer.Length);

            string deviceName = letter + ":";
            uint devicePathLength;

            fixed (char* pDeviceName = deviceName)
            {
                devicePathLength = PInvoke.QueryDosDevice(pDeviceName, pDevicePath, ushort.MaxValue);
            }

            if (devicePathLength == 0)
            {
                // The drive letter is not in use
                continue;
            }

            int usableDevicePathLength = (int)Math.Min(devicePathLength, (uint)devicePathBuffer.Length);

            string? target = VolumeHelper.ParseMultiString(
                    new string(devicePathBuffer, 0, usableDevicePathLength))
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(target))
            {
                continue;
            }

            // One level of \??\ indirection (e.g. SUBST X: C:\Dir) resolves against the
            // Mount Manager mappings collected above
            if (target!.StartsWith(@"\??\", StringComparison.Ordinal))
            {
                target = ResolveDosPathToDevicePath(list, target.Substring(4));

                if (target is null)
                {
                    continue;
                }
            }

            // Network redirector targets embed session-specific "\;X:" segments the driver
            // never sees in image paths; emitting them would create entries that match nothing
            if (!target.StartsWith(@"\Device\", StringComparison.OrdinalIgnoreCase) ||
                target.IndexOf(@"\;", StringComparison.Ordinal) >= 0)
            {
                continue;
            }

            list.Add(new VolumeMapping(mountPoint, string.Empty, target));
        }
    }

    /// <summary>
    ///     Translates a DOS path (e.g. "C:\Dir") to its NT device path using already collected mappings.
    /// </summary>
    private static string? ResolveDosPathToDevicePath(List<VolumeMapping> mappings, string dosPath)
    {
        VolumeMapping? bestMapping = null;
        string? bestMountPoint = null;

        foreach (VolumeMapping current in mappings)
        {
            string mountPoint = current.MountPoint.TrimEnd(Path.DirectorySeparatorChar);

            if (mountPoint.Length == 0)
            {
                continue;
            }

            bool isMatch =
                dosPath.Equals(mountPoint, StringComparison.OrdinalIgnoreCase) ||
                dosPath.StartsWith(mountPoint + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            if (bestMountPoint is null || mountPoint.Length > bestMountPoint.Length)
            {
                bestMapping = current;
                bestMountPoint = mountPoint;
            }
        }

        if (bestMapping is null || bestMountPoint is null)
        {
            return null;
        }

        string relativePath = dosPath.Length > bestMountPoint.Length
            ? dosPath.Substring(bestMountPoint.Length).TrimStart(Path.DirectorySeparatorChar)
            : string.Empty;

        string devicePath = bestMapping.DevicePath.TrimEnd(Path.DirectorySeparatorChar);

        return relativePath.Length == 0 ? devicePath : devicePath + Path.DirectorySeparatorChar + relativePath;
    }

    /// <summary>
    ///     Extracts the string content of a fixed-size buffer up to the first NUL terminator.
    /// </summary>
    /// <param name="value">The character buffer.</param>
    /// <returns>The content before the first NUL character, or the whole buffer if none is present.</returns>
    private static string GetNullTerminatedString(char[] value)
    {
        int length = Array.IndexOf(value, '\0');

        return length < 0
            ? new string(value)
            : new string(value, 0, length);
    }
}
