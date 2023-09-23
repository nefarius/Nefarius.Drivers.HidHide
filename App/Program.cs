using Nefarius.Drivers.HidHide;

HidHideControlService hh = new HidHideControlService();

while (true)
{
    try
    {
        Console.WriteLine($"IsActive: {hh.IsActive}");
    }
    catch (HidHideDriverAccessFailedException) { }

    Thread.Sleep(100);
}