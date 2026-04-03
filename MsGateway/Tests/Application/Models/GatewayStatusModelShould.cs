using MsGateway.Application.Models;

namespace MsGateway.Tests.Application.Models;

public sealed class GatewayStatusModelShould
{
    [Fact]
    public void CreateWithStatus()
    {
        var model = new GatewayStatusModel("ok");

        model.Status.Should().Be("ok");
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var model1 = new GatewayStatusModel("ok");
        var model2 = new GatewayStatusModel("ok");

        model1.Should().Be(model2);
    }

    [Fact]
    public void SupportDifferentStatuses()
    {
        var ok = new GatewayStatusModel("ok");
        var error = new GatewayStatusModel("error");

        ok.Should().NotBe(error);
    }
}
