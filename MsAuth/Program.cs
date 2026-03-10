using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MsAuth.Domain.Services;
using MsAuth.Infrastructure.Data;
using MsAuth.Infrastructure.Repositories;
using MsAuth.Infrastructure.Options;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection("Database"));
builder.Services.AddSingleton<IDapperContext, DapperContext>();
builder.Services.AddScoped<IUserRepository, UserSqlRepository>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenSqlRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddSingleton<IJwtService>(_ => new JwtService(
    jwtSection["SecretKey"],
    jwtSection["Issuer"],
    jwtSection["Audience"],
    int.Parse(jwtSection["AccessTokenExpirationMinutes"] ?? "15")
));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://webapp:80", "http://localhost:80")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy", service = "MsAuth" }));

app.UseCors("AllowWebApp");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();

public partial class Program { }
