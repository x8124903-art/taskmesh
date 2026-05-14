using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.Data;
using MsProjects.Infrastructure.Repositories;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Application.UseCases.Project;
using MsProjects.Application.UseCases.ProjectMember;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Infrastructure.HttpClients;
using MassTransit;
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
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MsProjects"))
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
builder.Services.AddSingleton<IDapperContext, DapperContext>();

var redisConnection = builder.Configuration.GetSection("RedisCache:ConnectionString").Value;
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "TaskMesh:Projects:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddScoped<IProjectRepository, ProjectSqlRepository>();
builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberSqlRepository>();
builder.Services.AddScoped<IProjectInvitationRepository, ProjectInvitationSqlRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
builder.Services.AddScoped<IProjectAuthorizationService, ProjectAuthorizationService>();
builder.Services.AddScoped<IProjectInvitationService, ProjectInvitationService>();

// Project UseCases
builder.Services.AddScoped<IGetUserProjectsUseCase, GetUserProjectsUseCase>();
builder.Services.AddScoped<IGetProjectDetailsUseCase, GetProjectDetailsUseCase>();
builder.Services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
builder.Services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
builder.Services.AddScoped<IDeleteProjectUseCase, DeleteProjectUseCase>();

// ProjectMember UseCases
builder.Services.AddScoped<IGetProjectMembersUseCase, GetProjectMembersUseCase>();
builder.Services.AddScoped<IGetProjectMemberRoleUseCase, GetProjectMemberRoleUseCase>();
builder.Services.AddScoped<IChangeProjectMemberRoleUseCase, ChangeProjectMemberRoleUseCase>();
builder.Services.AddScoped<IRemoveProjectMemberUseCase, RemoveProjectMemberUseCase>();

// ProjectInvitation UseCases
builder.Services.AddScoped<ICreateProjectInvitationUseCase, CreateProjectInvitationUseCase>();
builder.Services.AddScoped<IGetInvitationDetailsUseCase, GetInvitationDetailsUseCase>();
builder.Services.AddScoped<IGetInvitationByTokenUseCase, GetInvitationByTokenUseCase>();
builder.Services.AddScoped<IGetProjectPendingInvitationsUseCase, GetProjectPendingInvitationsUseCase>();
builder.Services.AddScoped<IGetMyInvitationsUseCase, GetMyInvitationsUseCase>();
builder.Services.AddScoped<IAcceptProjectInvitationUseCase, AcceptProjectInvitationUseCase>();
builder.Services.AddScoped<IRejectProjectInvitationUseCase, RejectProjectInvitationUseCase>();
builder.Services.AddScoped<ICancelProjectInvitationUseCase, CancelProjectInvitationUseCase>();

builder.Services.AddHttpClient<IAuthHttpClient, AuthHttpClient>("MsAuth", client =>
{
    var baseUrl = builder.Configuration["MsAuth:BaseUrl"];
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

builder.Services.AddExceptionHandler<MsProjects.Infrastructure.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://webapp:80", "http://localhost:80", "http://msgateway:8080")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy", service = "MsProjects" }));
app.MapPrometheusScrapingEndpoint();

app.UseExceptionHandler();

app.UseCors("AllowAll");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
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
