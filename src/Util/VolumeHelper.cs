#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Windows.Win32;
using Windows.Win32.Foundation;

using Microsoft.Extensions.Logging;

namespace Nefarius.Drivers.HidHide.Util;

/// <summary>
///     Path manipulation and volume helper methods.
/// </summary>
internal class VolumeHelper
{
    private static readonly Regex ExtractDevicePathPrefixRegex = new(
        @"^(\\Device\\HarddiskVolume\d*)\\.*",
        RegexOptions.IgnoreCase);

    private readonly ILogger<VolumeHelper>? _logger;
    private readonly IVolumeMappingProvider _volumeMappingProvider;

    internal VolumeHelper(ILogger<VolumeHelper>? logger)
        : this(logger, new WindowsVolumeMappingProvider())
    {
    }

    internal VolumeHelper(ILogger<VolumeHelper>? logger, IVolumeMappingProvider volumeMappingProvider)
    {
        _logger = logger;
        _volumeMappingProvider = volumeMappingProvider;
    }

    /// <summary>
    ///     Splits a Win32 MULTI_SZ string into individual entries.
    /// </summary>
    /// <param name="value">The MULTI_SZ string.</param>
    /// <returns>The non-empty values.</returns>
    internal static IReadOnlyList<string> ParseMultiString(string value)
    {
        return value
            .Split('\0')
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToArray();
    }

    private static string? NormalizePath(string path)
    {
        try
        {
            return Path
                .GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .ToUpperInvariant();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Translates a "DOS device" path to user-land path.
    /// </summary>
    /// <param name="devicePath">The DOS device path to convert.</param>
    /// <param name="throwOnError">Throw exception on any sort of parsing error if true, false returns null.</param>
    /// <returns>The user-land path.</returns>
    public string? DosDevicePathToPath(string devicePath, bool throwOnError = true)
    {
        _logger?.LogDebug("++ Resolving DOS device path {DevicePath} to path", devicePath);

        //
        // TODO: cover and test junctions!
        // 

        Match prefixMatch = ExtractDevicePathPrefixRegex.Match(devicePath);

        if (!prefixMatch.Success)
        {
            _logger?.LogDebug("Prefix {Prefix} didn't match path {DevicePath}",
                ExtractDevicePathPrefixRegex, devicePath);

            if (throwOnError)
            {
                throw new ArgumentException("Failed to parse provided device path prefix");
            }

            return null;
        }

        string prefix = prefixMatch.Groups[1].Value;
        _logger?.LogDebug("Extracted prefix: {Prefix}", prefix);

        VolumeMapping? mapping = _volumeMappingProvider
            .GetVolumeMappings()
            .FirstOrDefault(m => prefix.Equals(m.DevicePath, StringComparison.OrdinalIgnoreCase));

        if (mapping is null)
        {
            if (throwOnError)
            {
                throw new ArgumentException("Failed to translate provided path");
            }

            return null;
        }

        string relativePath = devicePath
            .Substring(mapping.DevicePath.Length)
            .TrimStart(Path.DirectorySeparatorChar);

        _logger?.LogDebug("Built relative path: {Path}", relativePath);

        string fullPath = Path.Combine(mapping.MountPoint, relativePath);

        _logger?.LogDebug("-- Returning path {Path} for device path {DevicePath}", fullPath, devicePath);

        return fullPath;
    }

    /// <summary>
    ///     Translates a user-land file path to "DOS device" path.
    /// </summary>
    /// <param name="path">The file path in normal namespace format.</param>
    /// <param name="throwOnError">Throw exception on any sort of parsing error if true, false returns null.</param>
    /// <returns>The device namespace path (DOS device).</returns>
    public string? PathToDosDevicePath(string path, bool throwOnError = true)
    {
        _logger?.LogDebug("++ Resolving path {Path} to DOS device path", path);

        if (!File.Exists(path))
        {
            _logger?.LogWarning("The provided path {Path} doesn't exist", path);

            if (throwOnError)
            {
                throw new ArgumentException($"The supplied file path {path} doesn't exist", nameof(path));
            }

            return null;
        }

        if (!TryFindBestMapping(path, out VolumeMapping? mapping, out string? relativePath) ||
            mapping is null ||
            relativePath is null)
        {
            _logger?.LogWarning("Failed to resolve path {Path}", path);

            if (throwOnError)
            {
                throw new IOException($"Couldn't resolve device path of path {path}");
            }

            return null;
        }

        StringBuilder fullDevicePath = new();
        fullDevicePath.AppendFormat("{0}{1}", mapping.DevicePath, Path.DirectorySeparatorChar);
        fullDevicePath.Append(relativePath.TrimStart(Path.DirectorySeparatorChar));

        _logger?.LogDebug("-- Returning device path {DevicePath} for path {Path}", fullDevicePath, path);

        return fullDevicePath.ToString();
    }

    private bool TryFindBestMapping(string path, out VolumeMapping? mapping, out string? relativePath)
    {
        string? normalizedPath = NormalizePath(path);
        VolumeMapping? bestMapping = null;
        string? bestNormalizedMountPoint = null;

        if (normalizedPath is null || normalizedPath.Length == 0)
        {
            mapping = null;
            relativePath = null;
            return false;
        }

        foreach (VolumeMapping current in _volumeMappingProvider.GetVolumeMappings())
        {
            if (string.IsNullOrWhiteSpace(current.MountPoint) || string.IsNullOrWhiteSpace(current.DevicePath))
            {
                continue;
            }

            string? normalizedMountPoint = NormalizePath(current.MountPoint);

            if (normalizedMountPoint is null || normalizedMountPoint.Length == 0)
            {
                _logger?.LogDebug("Skipping invalid mount point {MountPoint}", current.MountPoint);
                continue;
            }

            string currentNormalizedMountPoint = normalizedMountPoint;
            bool isMatch =
                normalizedPath.Equals(currentNormalizedMountPoint, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith(
                    currentNormalizedMountPoint + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            if (bestNormalizedMountPoint is null || currentNormalizedMountPoint.Length > bestNormalizedMountPoint.Length)
            {
                bestMapping = current;
                bestNormalizedMountPoint = currentNormalizedMountPoint;
            }
        }

        if (bestMapping is null)
        {
            mapping = null;
            relativePath = null;
            return false;
        }

        mapping = bestMapping;
        relativePath = GetPathRelativeToMountPoint(bestMapping.MountPoint, path);
        return true;
    }

    private static string GetPathRelativeToMountPoint(string mountPoint, string path)
    {
        string normalizedMountPoint = mountPoint.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        int startIndex = path.StartsWith(mountPoint, StringComparison.OrdinalIgnoreCase)
            ? mountPoint.Length
            : normalizedMountPoint.Length;

        if (startIndex >= path.Length)
        {
            return string.Empty;
        }

        return path.Substring(startIndex).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}

internal interface IVolumeMappingProvider
{
    IReadOnlyList<VolumeMapping> GetVolumeMappings();
}

internal sealed class VolumeMapping
{
    public VolumeMapping(string mountPoint, string volumeName, string devicePath)
    {
        MountPoint = mountPoint;
        VolumeName = volumeName;
        DevicePath = devicePath;
    }

    public string MountPoint { get; }

    public string VolumeName { get; }

    public string DevicePath { get; }
}

internal sealed class WindowsVolumeMappingProvider : IVolumeMappingProvider
{
    private static readonly HANDLE InvalidHandleValue = new(new IntPtr(-1));

    public unsafe IReadOnlyList<VolumeMapping> GetVolumeMappings()
    {
        char[] volumeName = new char[ushort.MaxValue];
        char[] pathName = new char[ushort.MaxValue];
        char[] mountPoint = new char[ushort.MaxValue];

        fixed (char* pVolumeName = volumeName)
        fixed (char* pPathName = pathName)
        fixed (char* pMountPoint = mountPoint)
        {
            HANDLE volumeHandle = PInvoke.FindFirstVolume(pVolumeName, ushort.MaxValue);
            List<VolumeMapping> list = new();

            if (volumeHandle == InvalidHandleValue)
            {
                return list;
            }

            try
            {
                do
                {
                    string volume = GetNullTerminatedString(volumeName);
                    Array.Clear(mountPoint, 0, mountPoint.Length);
                    Array.Clear(pathName, 0, pathName.Length);

                    if (!PInvoke.GetVolumePathNamesForVolumeName(
                            volume,
                            pMountPoint,
                            ushort.MaxValue,
                            out uint mountPointLength
                        ))
                    {
                        continue;
                    }

                    string multiString = new string(mountPoint, 0, (int)mountPointLength);
                    IReadOnlyList<string> mountPoints = VolumeHelper.ParseMultiString(multiString);

                    if (mountPoints.Count == 0)
                    {
                        continue;
                    }

                    string deviceName = volume.Substring(4, volume.Length - 1 - 4);
                    uint devicePathLength;

                    fixed (char* pDeviceName = deviceName)
                    {
                        devicePathLength = PInvoke.QueryDosDevice(pDeviceName, pPathName, ushort.MaxValue);
                    }

                    if (devicePathLength <= 0)
                    {
                        continue;
                    }

                    string devicePath = new string(pathName, 0, (int)devicePathLength).TrimEnd('\0');

                    foreach (string currentMountPoint in mountPoints)
                    {
                        list.Add(new VolumeMapping(currentMountPoint, volume, devicePath));
                    }
                } while (PInvoke.FindNextVolume(volumeHandle, pVolumeName, ushort.MaxValue));
            }
            finally
            {
                PInvoke.FindVolumeClose(volumeHandle);
            }

            return list;
        }
    }

    private static string GetNullTerminatedString(char[] value)
    {
        int length = Array.IndexOf(value, '\0');

        return length < 0
            ? new string(value)
            : new string(value, 0, length);
    }
}
