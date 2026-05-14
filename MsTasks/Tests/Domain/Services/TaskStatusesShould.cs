using FluentAssertions;
using MsTasks.Domain;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Domain.Services;

public sealed class TaskStatusesShould
{
    [Fact]
    public void All_ContainsAllEnumValues()
    {
        // Arrange
        var expected = Enum.GetValues<TaskStatus>();

        // Act
        var result = TaskStatuses.All;

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Should().Contain(TaskStatus.Todo);
        result.Should().Contain(TaskStatus.InProgress);
        result.Should().Contain(TaskStatus.Done);
        result.Should().Contain(TaskStatus.Blocked);
    }

    [Theory]
    [InlineData(TaskStatus.Todo, true)]
    [InlineData(TaskStatus.InProgress, true)]
    [InlineData(TaskStatus.Done, true)]
    [InlineData(TaskStatus.Blocked, true)]
    [InlineData((TaskStatus)999, false)]
    public void IsValid_ReturnsCorrectResult(TaskStatus status, bool expected)
    {
        // Act
        var result = TaskStatuses.IsValid(status);

        // Assert
        result.Should().Be(expected);
    }
}
