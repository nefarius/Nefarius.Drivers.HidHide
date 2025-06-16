using Nefarius.Drivers.HidHide;

namespace Tests;

internal partial class Tests
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
    public void TestAppListValid()
    {
        HidHideControlService service = new();

        service.ClearApplicationsList();

        // make sure this exists or an exception will be thrown
        const string fileName = @"E:\Downloads\amd-software-adrenalin-edition-24.6.1-minimalsetup-240626_web.exe";

        service.AddApplicationPath(fileName);

        List<string> list = service.ApplicationPaths.ToList();

        Assert.That(list, Contains.Item(fileName));
    }
    
    [Test]
    public void TestAppListInvalid()
    {
        HidHideControlService service = new();
        
        const string fileName = @"F:\Downloads\I-do-not-exist.exe";

        Assert.That(() => service.AddApplicationPath(fileName), Throws.TypeOf<FileNotFoundException>());
    }
}