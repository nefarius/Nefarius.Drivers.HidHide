using Microsoft.Extensions.DependencyInjection;

using Nefarius.Drivers.HidHide;

namespace Tests;

internal partial class Tests
{
    private IHidHideControlService _hhControl;
    private HidHideSetupProvider _hhProvider;

    [SetUp]
    public void Setup()
    {
        ServiceCollection sc = new();
        sc.AddHidHide();

        ServiceProvider sp = sc.BuildServiceProvider();

        _hhControl = sp.GetRequiredService<IHidHideControlService>();
        _hhProvider = sp.GetRequiredService<HidHideSetupProvider>();
    }

    [Test]
    public void TestIsActive()
    {
        _hhControl.IsActive = true;
        
        Assert.That(_hhControl.IsActive, Is.True);
        
        _hhControl.IsActive = false;
        
        Assert.That(_hhControl.IsActive, Is.False);
    }

    [Test]
    public void TestIsAppListInverted()
    {
        _hhControl.IsAppListInverted = true;

        Assert.That(_hhControl.IsAppListInverted, Is.True);

        _hhControl.IsAppListInverted = false;

        Assert.That(_hhControl.IsAppListInverted, Is.False);
    }

    [Test]
    public void TestAppListValid()
    {
        _hhControl.ClearApplicationsList();

        List<string> list = _hhControl.ApplicationPaths.ToList();

        Assert.That(list, Is.Empty);

        // make sure this exists or an exception will be thrown
        string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "rundll32.exe");

        _hhControl.AddApplicationPath(fileName);

        list = _hhControl.ApplicationPaths.ToList();

        Assert.That(list, Contains.Item(fileName));
    }

    [Test]
    public void TestAppListInvalid()
    {
        string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            Path.GetRandomFileName());

        Assert.That(() => _hhControl.AddApplicationPath(fileName), Throws.TypeOf<FileNotFoundException>());
    }
}