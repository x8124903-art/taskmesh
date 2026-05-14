using MsGateway.Domain.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

MsGateway.Startup.ConfigureServices(builder);
var app = builder.Build();
MsGateway.Startup.Configure(app);
app.Run();