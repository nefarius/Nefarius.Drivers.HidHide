using Nefarius.Drivers.HidHide.Util;

namespace Tests;

internal sealed class VolumeHelperTests
{
    [Test]
    public void PathToDosDevicePath_UsesLongestMatchingMountPoint()
    {
        using TempFile tempFile = TempFile.Create("Application.exe", "nested");
        string root = Path.GetPathRoot(tempFile.Path)!;
        string mountPoint = Path.GetDirectoryName(Path.GetDirectoryName(tempFile.Path)!)! + Path.DirectorySeparatorChar;

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"),
            new VolumeMapping(mountPoint, @"\\?\Volume{mount}\", @"\Device\HarddiskVolume2"));

        string? result = helper.PathToDosDevicePath(tempFile.Path);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume2\nested\Application.exe"));
    }

    [Test]
    public void PathToDosDevicePath_IgnoresMalformedMountPoint()
    {
        using TempFile tempFile = TempFile.Create("Application.exe");
        string root = Path.GetPathRoot(tempFile.Path)!;

        // The embedded NUL makes Path.GetFullPath throw ArgumentException on every target framework,
        // exercising the "skipping unusable mount point" branch instead of an ordinary prefix mismatch.
        VolumeHelper helper = CreateHelper(
            new VolumeMapping("C:\\Bro\0ken\\", @"\\?\Volume{broken}\", @"\Device\HarddiskVolume9"),
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.PathToDosDevicePath(tempFile.Path);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume1\" + Path.GetRelativePath(root, tempFile.Path)));
    }

    [Test]
    public void PathToDosDevicePath_SkipsMappingsWithoutMountPoint()
    {
        using TempFile tempFile = TempFile.Create("Application.exe");
        string root = Path.GetPathRoot(tempFile.Path)!;

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(string.Empty, @"\\?\Volume{none}\", @"\Device\HarddiskVolume9"),
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.PathToDosDevicePath(tempFile.Path);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume1\" + Path.GetRelativePath(root, tempFile.Path)));
    }

    [Test]
    public void PathToDosDevicePath_CanonicalizesParentDirectorySegments()
    {
        using TempFile tempFile = TempFile.Create("Application.exe", "nested");
        string root = Path.GetPathRoot(tempFile.Path)!;
        string directory = Path.GetDirectoryName(tempFile.Path)!;
        string dottedPath = Path.Combine(directory, "..", "nested", "Application.exe");

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.PathToDosDevicePath(dottedPath);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume1\" + Path.GetRelativePath(root, tempFile.Path)));
    }

    [Test]
    public void PathToDosDevicePath_CanonicalizesForwardSlashes()
    {
        using TempFile tempFile = TempFile.Create("Application.exe", "nested");
        string root = Path.GetPathRoot(tempFile.Path)!;
        string forwardSlashPath = tempFile.Path.Replace('\\', '/');

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.PathToDosDevicePath(forwardSlashPath);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume1\" + Path.GetRelativePath(root, tempFile.Path)));
    }

    [Test]
    public void PathToDosDevicePath_ReturnsNull_WhenNoMappingMatchesAndThrowOnErrorIsFalse()
    {
        using TempFile tempFile = TempFile.Create("Application.exe");
        string root = Path.GetPathRoot(tempFile.Path)!;
        string nonMatchingMountPoint =
            Path.Combine(root, "VolumeHelperTestsNoSuchMount") + Path.DirectorySeparatorChar;

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(nonMatchingMountPoint, @"\\?\Volume{other}\", @"\Device\HarddiskVolume9"));

        Assert.Multiple(() =>
        {
            Assert.That(helper.PathToDosDevicePath(tempFile.Path, throwOnError: false), Is.Null);
            Assert.That(() => helper.PathToDosDevicePath(tempFile.Path), Throws.InstanceOf<IOException>());
        });
    }

    [Test]
    public void DosDevicePathToPath_UsesFirstMappingOnEqualDevicePaths()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"),
            new VolumeMapping(root.TrimEnd(Path.DirectorySeparatorChar) + @"\Mount\", @"\\?\Volume{mount}\", @"\Device\HarddiskVolume1"));

        string? result = helper.DosDevicePathToPath(@"\Device\HarddiskVolume1\Apps\DSX.exe");

        Assert.That(result, Is.EqualTo(Path.Combine(root, "Apps", "DSX.exe")));
    }

    [Test]
    public void DosDevicePathToPath_MatchesDevicePathCaseInsensitively()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.DosDevicePathToPath(@"\device\harddiskvolume1\Apps\DSX.exe");

        Assert.That(result, Is.EqualTo(Path.Combine(root, "Apps", "DSX.exe")));
    }

    [Test]
    public void DosDevicePathToPath_SupportsNonHarddiskVolumeDevices()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{crypt}\", @"\Device\VeraCryptVolumeG"));

        string? result = helper.DosDevicePathToPath(@"\Device\VeraCryptVolumeG\Apps\Game.exe");

        Assert.That(result, Is.EqualTo(Path.Combine(root, "Apps", "Game.exe")));
    }

    [Test]
    public void DosDevicePathToPath_DoesNotTreatLongerVolumeNumberAsPrefixMatch()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        string mountPoint = root.TrimEnd(Path.DirectorySeparatorChar) + @"\Mount\";
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{one}\", @"\Device\HarddiskVolume1"),
            new VolumeMapping(mountPoint, @"\\?\Volume{ten}\", @"\Device\HarddiskVolume10"));

        Assert.Multiple(() =>
        {
            Assert.That(
                helper.DosDevicePathToPath(@"\Device\HarddiskVolume10\Apps\Game.exe"),
                Is.EqualTo(Path.Combine(mountPoint, "Apps", "Game.exe")));
            Assert.That(
                helper.DosDevicePathToPath(@"\Device\HarddiskVolume1\Apps\Game.exe"),
                Is.EqualTo(Path.Combine(root, "Apps", "Game.exe")));
        });
    }

    [Test]
    public void DosDevicePathToPath_ReturnsNull_WhenOnlyShorterVolumeNumberIsRegistered()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{one}\", @"\Device\HarddiskVolume1"));

        Assert.Multiple(() =>
        {
            Assert.That(
                helper.DosDevicePathToPath(@"\Device\HarddiskVolume10\Game.exe", throwOnError: false),
                Is.Null);
            Assert.That(
                () => helper.DosDevicePathToPath(@"\Device\HarddiskVolume10\Game.exe"),
                Throws.ArgumentException);
        });
    }

    [Test]
    public void DosDevicePathToPath_ReturnsNull_WhenNoMappingMatchesAndThrowOnErrorIsFalse()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        Assert.Multiple(() =>
        {
            Assert.That(helper.DosDevicePathToPath(@"\Device\Mup\Share\Game.exe", throwOnError: false), Is.Null);
            Assert.That(() => helper.DosDevicePathToPath(@"\Device\Mup\Share\Game.exe"), Throws.ArgumentException);
        });
    }

    [Test]
    public void DosDevicePathToPath_ResolvesDevicePathWithAndWithoutTrailingSeparator()
    {
        string root = Path.GetPathRoot(TestContext.CurrentContext.WorkDirectory)!;
        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        Assert.Multiple(() =>
        {
            Assert.That(helper.DosDevicePathToPath(@"\Device\HarddiskVolume1"), Is.EqualTo(root));
            Assert.That(helper.DosDevicePathToPath(@"\Device\HarddiskVolume1\"), Is.EqualTo(root));
        });
    }

    [Test]
    public void PathConversions_RoundTripMountedFolderMapping()
    {
        using TempFile tempFile = TempFile.Create("Application.exe", "mounted", "nested");
        string root = Path.GetPathRoot(tempFile.Path)!;
        string mountPoint = Path.GetDirectoryName(Path.GetDirectoryName(tempFile.Path)!)! + Path.DirectorySeparatorChar;

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"),
            new VolumeMapping(mountPoint, @"\\?\Volume{mount}\", @"\Device\HarddiskVolume2"));

        string? devicePath = helper.PathToDosDevicePath(tempFile.Path);
        string? path = helper.DosDevicePathToPath(devicePath!);
        string? roundTripDevicePath = helper.PathToDosDevicePath(path!);

        Assert.Multiple(() =>
        {
            Assert.That(devicePath, Is.EqualTo(@"\Device\HarddiskVolume2\nested\Application.exe"));
            Assert.That(path, Is.EqualTo(tempFile.Path));
            Assert.That(roundTripDevicePath, Is.EqualTo(devicePath));
        });
    }

    [Test]
    public void ParseMultiString_ReturnsIndividualMountPoints()
    {
        string value = "D:\\" + '\0' + "D:\\Mounted\\Steam\\" + '\0' + '\0';

        IReadOnlyList<string> result = VolumeHelper.ParseMultiString(value);

        Assert.That(result, Is.EqualTo(new[] { @"D:\", @"D:\Mounted\Steam\" }));
    }

    [Test]
    public void ParseMultiString_ReturnsEmptyList_WhenVolumeHasNoMountPoints()
    {
        string value = new('\0', 1);

        IReadOnlyList<string> result = VolumeHelper.ParseMultiString(value);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void WindowsProvider_EnumeratesSystemDriveMapping()
    {
        string systemRoot = Path.GetPathRoot(Environment.SystemDirectory)!;

        IReadOnlyList<VolumeMapping> mappings = new WindowsVolumeMappingProvider().GetVolumeMappings();

        VolumeMapping? systemMapping = mappings.FirstOrDefault(m =>
            m.MountPoint.Equals(systemRoot, StringComparison.OrdinalIgnoreCase));

        Assert.Multiple(() =>
        {
            Assert.That(systemMapping, Is.Not.Null);
            Assert.That(systemMapping!.DevicePath, Does.StartWith(@"\Device\"));
            Assert.That(mappings.Select(m => m.DevicePath), Has.None.Contains(@"\;"));
        });
    }

    private static VolumeHelper CreateHelper(params VolumeMapping[] mappings)
    {
        return new VolumeHelper(null, new TestVolumeMappingProvider(mappings));
    }

    private sealed class TestVolumeMappingProvider : IVolumeMappingProvider
    {
        private readonly IReadOnlyList<VolumeMapping> _mappings;

        public TestVolumeMappingProvider(IReadOnlyList<VolumeMapping> mappings)
        {
            _mappings = mappings;
        }

        public IReadOnlyList<VolumeMapping> GetVolumeMappings()
        {
            return _mappings;
        }
    }

    private sealed class TempFile : IDisposable
    {
        private readonly string _directory;

        private TempFile(string directory, string path)
        {
            _directory = directory;
            Path = path;
        }

        public string Path { get; }

        public static TempFile Create(string fileName, params string[] subdirectories)
        {
            string directory = System.IO.Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "VolumeHelperTests",
                Guid.NewGuid().ToString("N"));
            string rootDirectory = directory;

            foreach (string subdirectory in subdirectories)
            {
                directory = System.IO.Path.Combine(directory, subdirectory);
            }

            Directory.CreateDirectory(directory);
            string path = System.IO.Path.Combine(directory, fileName);
            File.WriteAllText(path, string.Empty);

            return new TempFile(rootDirectory, path);
        }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }

            try
            {
                // Remove the shared parent directory once the last test directory is gone
                Directory.Delete(System.IO.Path.GetDirectoryName(_directory)!, recursive: false);
            }
            catch (IOException)
            {
                // Another test still owns a sibling directory; leave the shared parent in place
            }
        }
    }
}
