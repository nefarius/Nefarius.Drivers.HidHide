using Nefarius.Drivers.HidHide;

HidHideControlService hh = new HidHideControlService();

var t = hh.ApplicationPaths.ToList();

hh.AddApplicationPath(@"F:\Downloads\windowsdesktop-runtime-7.0.12-win-x64.exe");
