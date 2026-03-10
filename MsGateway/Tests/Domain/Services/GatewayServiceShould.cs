using MsGateway.Domain.Services;

namespace MsGateway.Tests.Domain.Services;

public sealed class GatewayServiceShould
{
    private readonly GatewayService _sut = new();

    [Fact]
    public void GetStatus_ReturnsOk()
    {
        var result = _sut.GetStatus();

        result.Should().Be("ok");
    }

    [Fact]
    public void GetStatus_ReturnsNonEmptyString()
    {
        var result = _sut.GetStatus();

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ImplementIGatewayService()
    {
        _sut.Should().BeAssignableTo<IGatewayService>();
    }
}
