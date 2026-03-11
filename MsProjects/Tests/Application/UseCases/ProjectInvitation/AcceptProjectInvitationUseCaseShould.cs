using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Domain.Services;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectInvitation;

public sealed class AcceptProjectInvitationUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly AcceptProjectInvitationUseCase _useCase;

    public AcceptProjectInvitationUseCaseShould()
    {
        _useCase = new AcceptProjectInvitationUseCase(_invitationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToService()
    {
        _invitationServiceMock.Setup(x => x.AcceptInvitationAsync("tok", 1, It.IsAny<CancellationToken>(), "John", "john@demo.com"))
            .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync("tok", 1, "John", "john@demo.com", CancellationToken.None);

        _invitationServiceMock.Verify(x => x.AcceptInvitationAsync("tok", 1, It.IsAny<CancellationToken>(), "John", "john@demo.com"), Times.Once);
    }
}
