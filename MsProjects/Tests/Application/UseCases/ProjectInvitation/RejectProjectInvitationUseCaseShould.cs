using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MsProjects.Application.UseCases.ProjectInvitation;
using MsProjects.Domain.Services;
using Xunit;

namespace MsProjects.Tests.Application.UseCases.ProjectInvitation;

public sealed class RejectProjectInvitationUseCaseShould
{
    private readonly Mock<IProjectInvitationService> _invitationServiceMock = new();
    private readonly RejectProjectInvitationUseCase _useCase;

    public RejectProjectInvitationUseCaseShould()
    {
        _useCase = new RejectProjectInvitationUseCase(_invitationServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DelegatesToService()
    {
        _invitationServiceMock.Setup(x => x.RejectInvitationAsync("tok", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _useCase.ExecuteAsync("tok", CancellationToken.None);

        _invitationServiceMock.Verify(x => x.RejectInvitationAsync("tok", It.IsAny<CancellationToken>()), Times.Once);
    }
}
