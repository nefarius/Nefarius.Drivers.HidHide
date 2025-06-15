using Nefarius.Drivers.HidHide;

namespace Tests;

internal class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestIsActive()
    {
        HidHideControlService service = new();

        Assert.That(service.IsActive, Is.True);
    }

    [Test]
    public void TestIsAppListInverted()
    {
        HidHideControlService service = new();

        Assert.That(service.IsAppListInverted, Is.True);
    }

    [Test]
    public void TestAppList()
    {
        HidHideControlService service = new();

        service.ClearApplicationsList();

        // make sure this exists or an exception will be thrown
        //const string fileName = @"F:\Downloads\amd-software-adrenalin-edition-22.10.1-minimalsetup-221003_web.exe";
        const string fileName = @"E:\Downloads\amd-software-adrenalin-edition-24.6.1-minimalsetup-240626_web.exe";

        service.AddApplicationPath(fileName);

        List<string> list = service.ApplicationPaths.ToList();

        Assert.That(list, Contains.Item(fileName));
    }
}