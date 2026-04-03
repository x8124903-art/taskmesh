using Microsoft.AspNetCore.Mvc;
using MsGateway.Application.Controllers;

namespace MsGateway.Tests.Application.Controllers;

public sealed class GatewayControllerShould
{
    private readonly GatewayController _sut = new();

    [Fact]
    public void Health_ReturnsOk()
    {
        var result = _sut.Health();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Health_ReturnsObjectWithStatusOk()
    {
        var result = _sut.Health() as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();

        var json = System.Text.Json.JsonSerializer.Serialize(result.Value);
        json.Should().Contain("\"status\"");
        json.Should().Contain("\"ok\"");
    }

    [Fact]
    public void BeDecoratedWithApiControllerAttribute()
    {
        typeof(GatewayController)
            .Should().BeDecoratedWith<ApiControllerAttribute>();
    }

    [Fact]
    public void BeDecoratedWithRouteAttribute()
    {
        typeof(GatewayController)
            .Should().BeDecoratedWith<RouteAttribute>(
                attr => attr.Template == "api/v{version:apiVersion}/gateway");
    }

    [Fact]
    public void BeSealed()
    {
        typeof(GatewayController).Should().BeSealed();
    }

    [Fact]
    public void InheritFromControllerBase()
    {
        _sut.Should().BeAssignableTo<ControllerBase>();
    }
}
