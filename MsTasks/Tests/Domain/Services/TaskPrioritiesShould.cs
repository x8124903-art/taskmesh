using FluentAssertions;
using MsTasks.Domain;
using MsTasks.Domain.Services;

namespace MsTasks.Tests.Domain.Services;

public sealed class TaskPrioritiesShould
{
    [Fact]
    public void All_ContainsAllEnumValues()
    {
        // Arrange
        var expected = Enum.GetValues<TaskPriority>();

        // Act
        var result = TaskPriorities.All;

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Should().Contain(TaskPriority.Low);
        result.Should().Contain(TaskPriority.Medium);
        result.Should().Contain(TaskPriority.High);
    }

    [Theory]
    [InlineData(TaskPriority.Low, true)]
    [InlineData(TaskPriority.Medium, true)]
    [InlineData(TaskPriority.High, true)]
    [InlineData((TaskPriority)999, false)]
    public void IsValid_ReturnsCorrectResult(TaskPriority priority, bool expected)
    {
        // Act
        var result = TaskPriorities.IsValid(priority);

        // Assert
        result.Should().Be(expected);
    }
}
