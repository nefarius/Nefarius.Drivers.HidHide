using App;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Nefarius.Drivers.HidHide;

using Serilog;
using Serilog.Events;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(LogEventLevel.Debug)
    .CreateLogger();
builder.Logging.AddSerilog();

builder.Services.AddHidHide();

builder.Services.AddHostedService<DemoService>();

IHost app = builder.Build();

app.Run();