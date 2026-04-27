using FluentAssertions;
using Moq;
using MsTasks.Application.Models;
using MsTasks.Application.UseCases.Task;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Application.UseCases.Task;

public sealed class GetBoardTasksUseCaseShould
{
    [Fact]
    public async System.Threading.Tasks.Task ExecuteAsync_DelegatesToTaskService()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        var columns = new Dictionary<string, List<TaskModel>>();
        foreach (var status in TaskStatuses.All)
        {
            columns[status.ToString()] = new List<TaskModel>();
        }
        var expected = new BoardResponse(columns);

        var serviceMock = new Mock<ITaskService>();
        serviceMock
            .Setup(x => x.GetBoardAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new GetBoardTasksUseCase(serviceMock.Object);

        // Act
        var result = await useCase.ExecuteAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        result.Columns.Should().HaveCount(6);
    }
}
