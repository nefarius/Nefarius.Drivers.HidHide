using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace Nefarius.Drivers.HidHide.Util;

/// <summary>
///     Path manipulation and volume helper methods.
/// </summary>
public static class VolumeHelper
{
    private static readonly Regex ExtractDevicePathPrefixRegex = new(@"^(\\Device\\HarddiskVolume\d*)\\.*");

    /// <summary>
    ///     Curates and returns a collection of volume to path mappings.
    /// </summary>
    /// <returns>A collection of <see cref="VolumeMeta" />.</returns>
    private static unsafe IEnumerable<VolumeMeta> GetVolumeMappings()
    {
        char[] volumeName = new char[ushort.MaxValue];
        char[] pathName = new char[ushort.MaxValue];
        char[] mountPoint = new char[ushort.MaxValue];

        fixed (char* pVolumeName = volumeName)
        fixed (char* pPathName = pathName)
        fixed (char* pMountPoint = mountPoint)
        {
            HANDLE volumeHandle = PInvoke.FindFirstVolume(pVolumeName, ushort.MaxValue);

            List<VolumeMeta> list = new();

            do
            {
                string volume = new string(volumeName).TrimEnd('\0');

                if (!PInvoke.GetVolumePathNamesForVolumeName(
                        volume,
                        pMountPoint,
                        ushort.MaxValue,
                        out uint returnLength
                    ))
                {
                    continue;
                }

                // Extract volume name for use with QueryDosDevice
                string deviceName = volume.Substring(4, volume.Length - 1 - 4);

                // Grab device path
                returnLength = PInvoke.QueryDosDevice(deviceName, pPathName, ushort.MaxValue);

                if (returnLength <= 0)
                {
                    continue;
                }

                VolumeMeta entry = new()
                {
                    DriveLetter = new string(mountPoint).TrimEnd('\0'),
                    VolumeName = volume,
                    DevicePath = new string(pathName).TrimEnd('\0')
                };

                list.Add(entry);
            } while (PInvoke.FindNextVolume(volumeHandle, pVolumeName, ushort.MaxValue));

            return list.ToArray();
        }
    }

    /// <summary>
    ///     Checks if a path is a junction point.
    /// </summary>
    /// <param name="di">A <see cref="FileSystemInfo" /> instance.</param>
    /// <returns>True if it's a junction, false otherwise.</returns>
    private static bool IsPathReparsePoint(FileSystemInfo di)
    {
        return di.Attributes.HasFlag(FileAttributes.ReparsePoint);
    }

    /// <summary>
    ///     Helper to make paths comparable.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>The normalized path.</returns>
    private static string NormalizePath(string path)
    {
        return Path
            .GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    /// <summary>
    ///     Translates a "DOS device" path to user-land path.
    /// </summary>
    /// <param name="devicePath">The DOS device path to convert.</param>
    /// <returns>The user-land path.</returns>
    public static string DosDevicePathToPath(string devicePath)
    {
        //
        // TODO: cover and test junctions!
        // 

        Match prefixMatch = ExtractDevicePathPrefixRegex.Match(devicePath);

        if (!prefixMatch.Success)
        {
            throw new ArgumentException("Failed to parse provided device path prefix");
        }

        string prefix = prefixMatch.Groups[1].Value;

        VolumeMeta mapping = GetVolumeMappings()
            .SingleOrDefault(m => prefix.Equals(m.DevicePath));

        if (mapping is null)
        {
            throw new ArgumentException("Failed to translate provided path");
        }

        string relativePath = devicePath
            .Replace(mapping.DevicePath, string.Empty)
            .TrimStart(Path.DirectorySeparatorChar);

        return Path.Combine(mapping.DriveLetter, relativePath);
    }

    /// <summary>
    ///     Translates a user-land file path to "DOS device" path.
    /// </summary>
    /// <param name="path">The file path in normal namespace format.</param>
    /// <returns>The device namespace path (DOS device).</returns>
    public static string PathToDosDevicePath(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException($"The supplied file path {path} doesn't exist", nameof(path));
        }

        string filePart = Path.GetFileName(path);
        string pathPart = Path.GetDirectoryName(path);

        if (string.IsNullOrEmpty(pathPart))
        {
            throw new IOException($"Couldn't resolve directory of path {path}");
        }

        string pathNoRoot = string.Empty;
        string devicePath = string.Empty;

        // Walk up the directory tree to get the "deepest" potential junction
        for (DirectoryInfo current = new(pathPart);
             current is { Exists: true };
             current = Directory.GetParent(current.FullName))
        {
            if (!IsPathReparsePoint(current))
            {
                continue;
            }

            devicePath = GetVolumeMappings()
                .FirstOrDefault(m =>
                    !string.IsNullOrEmpty(m.DriveLetter) &&
                    NormalizePath(m.DriveLetter) == NormalizePath(current.FullName))
                ?.DevicePath;

            pathNoRoot = pathPart.Substring(current.FullName.Length);

            break;
        }

        // No junctions found, translate original path
        if (string.IsNullOrEmpty(devicePath))
        {
            string driveLetter = Path.GetPathRoot(pathPart);
            devicePath = GetVolumeMappings()
                .FirstOrDefault(m =>
                    m.DriveLetter.Equals(driveLetter, StringComparison.InvariantCultureIgnoreCase))?.DevicePath;
            pathNoRoot = pathPart.Substring(Path.GetPathRoot(pathPart).Length);
        }

        if (string.IsNullOrEmpty(devicePath))
        {
            throw new IOException($"Couldn't resolve device path of path {path}");
        }

        StringBuilder fullDevicePath = new();

        // Build new DOS Device path
        fullDevicePath.AppendFormat("{0}{1}", devicePath, Path.DirectorySeparatorChar);
        fullDevicePath.Append(Path.Combine(pathNoRoot, filePart).TrimStart(Path.DirectorySeparatorChar));

        return fullDevicePath.ToString();
    }

    private class VolumeMeta
    {
        public string DriveLetter { get; set; }

        public string VolumeName { get; set; }

        public string DevicePath { get; set; }
    }
}