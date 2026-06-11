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
    public void PathToDosDevicePath_IgnoresMalformedMountPoint_WhenThrowOnErrorIsFalse()
    {
        using TempFile tempFile = TempFile.Create("Application.exe");
        string root = Path.GetPathRoot(tempFile.Path)!;

        VolumeHelper helper = CreateHelper(
            new VolumeMapping(@"1:\Broken\", @"\\?\Volume{broken}\", @"\Device\HarddiskVolume9"),
            new VolumeMapping(root, @"\\?\Volume{root}\", @"\Device\HarddiskVolume1"));

        string? result = helper.PathToDosDevicePath(tempFile.Path, throwOnError: false);

        Assert.That(result, Is.EqualTo(@"\Device\HarddiskVolume1\" + Path.GetRelativePath(root, tempFile.Path)));
    }

    [Test]
    public void DosDevicePathToPath_UsesFirstMatchingVolumeMapping()
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
        }
    }
}
