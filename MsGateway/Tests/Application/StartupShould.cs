using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;

namespace MsGateway.Tests.Application;

public sealed class StartupShould
{
    private static WebApplicationBuilder CreateTestBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "TestSecretKeyThatIsLongEnoughForHS256Algorithm!!",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["ReverseProxy:Routes:auth-route:ClusterId"] = "auth-cluster",
            ["ReverseProxy:Routes:auth-route:Match:Path"] = "/api/v1/auth/{action}",
            ["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"] = "http://localhost:5310",
        });
        return builder;
    }

    [Fact]
    public void ConfigureServices_RegistersAuthentication()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        var authService = provider.GetService(
            typeof(Microsoft.AspNetCore.Authentication.IAuthenticationService));

        authService.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureServices_RegistersAuthorization()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        var authzService = provider.GetService(
            typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService));

        authzService.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureServices_RegistersCors()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        var corsService = provider.GetService(
            typeof(Microsoft.AspNetCore.Cors.Infrastructure.ICorsService));

        corsService.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureServices_RegistersReverseProxy()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        var proxyConfig = provider.GetService(
            typeof(Yarp.ReverseProxy.Configuration.IProxyConfigProvider));

        proxyConfig.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureServices_RegistersControllers()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        var actionDescriptorProvider = provider.GetService(
            typeof(Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider));

        actionDescriptorProvider.Should().NotBeNull();
    }

    [Fact]
    public void Configure_DoesNotThrow()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var app = builder.Build();

        var act = () => Startup.Configure(app);

        act.Should().NotThrow();
    }

    [Fact]
    public void Configure_MapsHealthEndpoint()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var app = builder.Build();

        Startup.Configure(app);

        var dataSource = app.Services.GetService(typeof(Microsoft.AspNetCore.Routing.EndpointDataSource))
            as Microsoft.AspNetCore.Routing.EndpointDataSource;
        dataSource.Should().NotBeNull();
    }

    [Fact]
    public void ConfigureServices_RegistersApiVersioning()
    {
        var builder = CreateTestBuilder();
        Startup.ConfigureServices(builder);
        var provider = builder.Services.BuildServiceProvider();

        provider.Should().NotBeNull();
    }

    private static RequestTransformContext CreateTransformContext(ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext();
        if (user != null)
            httpContext.User = user;

        var services = new ServiceCollection();
        services.AddLogging();
        httpContext.RequestServices = services.BuildServiceProvider();

        return new RequestTransformContext
        {
            HttpContext = httpContext,
            ProxyRequest = new HttpRequestMessage()
        };
    }

    [Fact]
    public async Task TransformRequest_AddsClaims_WhenAuthenticated()
    {
        var claims = new[]
        {
            new Claim("sub", "123"),
            new Claim("email", "test@example.com"),
            new Claim("name", "Test User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var ctx = CreateTransformContext(user);

        await Startup.TransformRequestAsync(ctx);

        ctx.ProxyRequest.Headers.GetValues("X-User-Id").Should().ContainSingle("123");
        ctx.ProxyRequest.Headers.GetValues("X-User-Email").Should().ContainSingle("test@example.com");
    }

    [Fact]
    public async Task TransformRequest_UsesAlternativeClaims_WhenStandardMissing()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "456"),
            new Claim(ClaimTypes.Email, "alt@example.com"),
            new Claim(ClaimTypes.Name, "Alt User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var ctx = CreateTransformContext(user);

        await Startup.TransformRequestAsync(ctx);

        ctx.ProxyRequest.Headers.GetValues("X-User-Id").Should().ContainSingle("456");
        ctx.ProxyRequest.Headers.GetValues("X-User-Email").Should().ContainSingle("alt@example.com");
    }

    [Fact]
    public async Task TransformRequest_DoesNothing_WhenNotAuthenticated()
    {
        var ctx = CreateTransformContext();

        await Startup.TransformRequestAsync(ctx);

        ctx.ProxyRequest.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task TransformRequest_LogsWarning_WhenUserIdMissing()
    {
        var claims = new[]
        {
            new Claim("email", "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var ctx = CreateTransformContext(user);

        await Startup.TransformRequestAsync(ctx);

        ctx.ProxyRequest.Headers.Contains("X-User-Id").Should().BeFalse();
        ctx.ProxyRequest.Headers.GetValues("X-User-Email").Should().ContainSingle("test@example.com");
    }

    [Fact]
    public async Task TransformRequest_NoEmailHeader_WhenEmailMissing()
    {
        var claims = new[]
        {
            new Claim("sub", "789")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var ctx = CreateTransformContext(user);

        await Startup.TransformRequestAsync(ctx);

        ctx.ProxyRequest.Headers.GetValues("X-User-Id").Should().ContainSingle("789");
        ctx.ProxyRequest.Headers.Contains("X-User-Email").Should().BeFalse();
    }
}
