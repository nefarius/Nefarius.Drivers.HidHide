using System.Runtime.InteropServices;

using App;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Nefarius.Drivers.HidHide;

using Serilog;
using Serilog.Events;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

// adds Serilog to demonstrate library logging capabilities
builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // most verbose stuff is Debug level
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(LogEventLevel.Debug)
    .CreateLogger();
builder.Logging.AddSerilog();

// adds all injectable types as services
builder.Services.AddHidHide(options =>
{
    options.ProcessArchitecture = Architecture.Arm64;
});

// runs example code
builder.Services.AddHostedService<DemoService>();

IHost app = builder.Build();

app.Run();