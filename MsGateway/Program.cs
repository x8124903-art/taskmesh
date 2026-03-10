using MsGateway.Domain.Services;

var builder = WebApplication.CreateBuilder(args);
MsGateway.Startup.ConfigureServices(builder);
var app = builder.Build();
MsGateway.Startup.Configure(app);
app.Run();