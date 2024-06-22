# <img src="assets/NSS-128x128.png" align="left" />Nefarius.Drivers.HidHide

[![.NET](https://github.com/nefarius/Nefarius.Drivers.HidHide/actions/workflows/build.yml/badge.svg)](https://github.com/nefarius/Nefarius.Drivers.HidHide/actions/workflows/build.yml) ![Requirements](https://img.shields.io/badge/Requires-.NET%206%2F7%2F8-blue.svg) ![Requirements](https://img.shields.io/badge/Requires-.NET%20Standard%202.0-blue.svg) [![Nuget](https://img.shields.io/nuget/v/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/) [![Nuget](https://img.shields.io/nuget/dt/Nefarius.Drivers.HidHide)](https://www.nuget.org/packages/Nefarius.Drivers.HidHide/)

Managed API for configuring [HidHide](https://github.com/nefarius/HidHide).

## Documentation

[Link to API docs](docs/index.md).

### Generating documentation

```PowerShell
dotnet build -c:Release
dotnet tool install --global Nefarius.Tools.XMLDoc2Markdown
xmldoc2md .\bin\net7-windows\Nefarius.Drivers.HidHide.dll .\docs\
```

## Usage (classic)

> This is the deprecated approach.

Create an instance of `HidHideControlService` whenever you need it.

This class **will not block other configuration apps** so you can keep it in memory as long as you need. A handle to the
driver is only opened when necessary (when the properties are read from or the methods get invoked).

## Usage (dependency injection)

> This is the recommended approach.

If you plan to make use of Microsoft Dependency Injection (in ASP.NET Core, Worker Services and alike) and the online
services, your app also need to add these NuGet packages:

- `Microsoft.Extensions.Http`
- `NJsonSchema`

You can skip them for the "traditional" use of version 1 of the library. Register it like:

```csharp
builder.Services.AddHidHide();
```

Now you can inject and consume:

- `IHidHideControlService` for the HidHide settings API
- `HidHideSetupProvider` for update and download information

Check the [demo app sources](./App) for implementation and usage details.

## 3rd party sources

- [C#/Win32 P/Invoke Source Generator](https://github.com/microsoft/CsWin32)
- [MinVer](https://github.com/adamralph/minver)
