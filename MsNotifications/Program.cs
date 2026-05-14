using MassTransit;
using MsNotifications.Application.UseCases.Notifications;
using MsNotifications.Domain.Services;
using MsNotifications.Infrastructure;
using MsNotifications.Infrastructure.Consumers;
using MsNotifications.Infrastructure.Data;
using MsNotifications.Infrastructure.Options;
using MsNotifications.Infrastructure.Repositories;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// OpenTelemetry
var otlpEndpoint = builder.Configuration["Observability:OtlpEndpoint"];

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MsNotifications"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrEmpty(otlpEndpoint))
        {
            tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// Options
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

// Infrastructure
builder.Services.AddScoped<IDapperContext, DapperContext>();
builder.Services.AddScoped<INotificationRepository, NotificationSqlRepository>();
builder.Services.AddScoped<IProcessedEventRepository, ProcessedEventSqlRepository>();

// Domain Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Use Cases
builder.Services.AddScoped<IGetNotificationsUseCase, GetNotificationsUseCase>();
builder.Services.AddScoped<IGetUnreadCountUseCase, GetUnreadCountUseCase>();
builder.Services.AddScoped<IMarkNotificationAsReadUseCase, MarkNotificationAsReadUseCase>();
builder.Services.AddScoped<IMarkAllNotificationsAsReadUseCase, MarkAllNotificationsAsReadUseCase>();
builder.Services.AddScoped<IDeleteNotificationUseCase, DeleteNotificationUseCase>();
builder.Services.AddScoped<ICreateNotificationUseCase, CreateNotificationUseCase>();

// MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<TaskAssignedEventConsumer>();
    x.AddConsumer<TaskStatusChangedEventConsumer>();
    x.AddConsumer<TaskCommentAddedEventConsumer>();
    x.AddConsumer<MemberInvitedEventConsumer>();
    x.AddConsumer<MemberJoinedEventConsumer>();
    x.AddConsumer<MemberRemovedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPass);
        });

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddMassTransitHostedService(true);

// Controllers
builder.Services.AddControllers();

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();

// Exception handling
app.UseExceptionHandler();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
