namespace MsProjects.Tests.Application.Models;

using MsProjects.Application.Models;

public sealed class RejectInvitationRequestShould
{
    [Fact]
    public void CreateWithValidToken()
    {
        var request = new RejectInvitationRequest("test-token-123");
        
        request.Token.Should().Be("test-token-123");
    }

    [Fact]
    public void CreateWithEmptyToken()
    {
        var request = new RejectInvitationRequest("");
        
        request.Token.Should().BeEmpty();
    }

    [Fact]
    public void SupportRecordEquality()
    {
        var request1 = new RejectInvitationRequest("token-123");
        var request2 = new RejectInvitationRequest("token-123");
        var request3 = new RejectInvitationRequest("token-456");
        
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }
}
