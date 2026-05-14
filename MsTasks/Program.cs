using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.OpenApi.Models;
using MassTransit;
using MsTasks.Application.UseCases.Task;
using MsTasks.Application.UseCases.TaskComment;
using MsTasks.Domain.Services;
using MsTasks.Infrastructure;
using MsTasks.Infrastructure.Data;
using MsTasks.Infrastructure.EventBus;
using MsTasks.Infrastructure.HttpClients;
using MsTasks.Infrastructure.Options;
using MsTasks.Infrastructure.Repositories;
using Polly;
using Polly.Extensions.Http;
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
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MsTasks"))
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

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));

builder.Services.AddScoped<IDapperContext, DapperContext>();
builder.Services.AddScoped<ITaskRepository, TaskSqlRepository>();
builder.Services.AddScoped<ITaskCommentRepository, TaskCommentSqlRepository>();

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskCommentService, TaskCommentService>();

builder.Services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
builder.Services.AddScoped<IGetTaskUseCase, GetTaskUseCase>();
builder.Services.AddScoped<IGetTasksByProjectUseCase, GetTasksByProjectUseCase>();
builder.Services.AddScoped<IUpdateTaskUseCase, UpdateTaskUseCase>();
builder.Services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
builder.Services.AddScoped<IAssignTaskUseCase, AssignTaskUseCase>();
builder.Services.AddScoped<IChangeTaskStatusUseCase, ChangeTaskStatusUseCase>();
builder.Services.AddScoped<IGetBoardTasksUseCase, GetBoardTasksUseCase>();

builder.Services.AddScoped<IAddTaskCommentUseCase, AddTaskCommentUseCase>();
builder.Services.AddScoped<IGetTaskCommentsUseCase, GetTaskCommentsUseCase>();

builder.Services.AddControllers();

var redisCacheEnabled = builder.Configuration.GetValue<bool>("RedisCache:Enabled");
if (redisCacheEnabled)
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["RedisCache:ConnectionString"];
        options.InstanceName = builder.Configuration["RedisCache:InstanceName"];
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddHttpClient<IProjectHttpClient, ProjectHttpClient>("MsProjects", client =>
{
    var baseUrl = builder.Configuration["MsProjects:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var connectionString = builder.Configuration["EventBus:ConnectionString"] 
            ?? throw new InvalidOperationException("EventBus:ConnectionString not configured");
        cfg.Host(new Uri(connectionString));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IEventBus, RabbitMqEventBus>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MsTasks API",
        Version = "v1",
        Description = "TaskMesh - Tasks Microservice"
    });
});

var app = builder.Build();

app.MapPrometheusScrapingEndpoint();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MsTasks API v1");
    });
}

app.UseRouting();

app.MapControllers();

app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "MsTasks" }));

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromMilliseconds(500));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}

public partial class Program { }
