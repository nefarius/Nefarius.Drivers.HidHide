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

        Assert.That(service.IsActive, Is.True);
    }

    [Test]
    public void TestIsAppListInverted()
    {
        var service = new HidHideControlService();

        Assert.That(service.IsAppListInverted, Is.True);
    }

    [Test]
    public void TestAppList()
    {
        var service = new HidHideControlService();

        service.ClearApplicationsList();

        // make sure this exists or an exception will be thrown
        var fileName = @"F:\Downloads\amd-software-adrenalin-edition-22.10.1-minimalsetup-221003_web.exe";

        service.AddApplicationPath(fileName);

        var list = service.ApplicationPaths.ToList();

        Assert.That(list, Contains.Item(fileName));
    }
}