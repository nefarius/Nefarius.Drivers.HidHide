using Nefarius.Drivers.HidHide;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestIsActive()
    {
        var service = new HidHideControlService();

        Assert.True(service.IsActive);
    }

    [Test]
    public void TestIsAppListInverted()
    {
        var service = new HidHideControlService();

        Assert.True(service.IsAppListInverted);
    }

    [Test]
    public void TestAppList()
    {
        var service = new HidHideControlService();

        // make sure this exists or an exception will be thrown
        var fileName = @"C:\temp\NVRTXVoice\setup.exe";

        service.AddApplicationPath(fileName);

        var list = service.ApplicationPaths.ToList();

        Assert.Contains(fileName, list);
    }
}