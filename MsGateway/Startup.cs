using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;

namespace MsGateway;

public static class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddApiVersioning();

        var jwtSection = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAuthentication", policy => policy.RequireAuthenticatedUser());
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://webapp:80", "http://localhost:80")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddTransforms(context =>
            {
                context.AddRequestTransform(TransformRequestAsync);
            });
    }

    internal static ValueTask TransformRequestAsync(RequestTransformContext transformContext)
    {
        var httpContext = transformContext.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst("sub")?.Value
                       ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = httpContext.User.FindFirst("email")?.Value
                      ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var name = httpContext.User.FindFirst("name")?.Value
                     ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.Add("X-User-Id", userId);
            }
            else
            {
                logger.LogWarning("UserId is null or empty - cannot add X-User-Id header");
            }
            
            if (!string.IsNullOrEmpty(email))
                transformContext.ProxyRequest.Headers.Add("X-User-Email", email);
            
            if (!string.IsNullOrEmpty(name))
                transformContext.ProxyRequest.Headers.Add("X-User-Name", name);
        }

        return ValueTask.CompletedTask;
    }

    public static void Configure(WebApplication app)
    {
        app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy", service = "MsGateway" }));

        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapReverseProxy();
    }
}
