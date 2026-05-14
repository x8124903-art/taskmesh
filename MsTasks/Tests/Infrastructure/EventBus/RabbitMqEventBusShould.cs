using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using MsTasks.Infrastructure.EventBus;
using Xunit;

namespace MsTasks.Tests.Infrastructure.EventBus;

public sealed class RabbitMqEventBusShould
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<RabbitMqEventBus>> _loggerMock;
    private readonly RabbitMqEventBus _sut;

    public RabbitMqEventBusShould()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<RabbitMqEventBus>>();
        _sut = new RabbitMqEventBus(_publishEndpointMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_CallsPublishEndpoint()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };

        // Act
        await _sut.PublishAsync(testEvent);

        // Assert
        _publishEndpointMock.Verify(
            x => x.Publish(testEvent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_LogsSuccessMessage()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };

        // Act
        await _sut.PublishAsync(testEvent);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Published event TestEvent to RabbitMQ")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ThrowsAndLogsError_WhenPublishFails()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        var expectedException = new Exception("Publish failed");
        _publishEndpointMock
            .Setup(x => x.Publish(testEvent, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.PublishAsync(testEvent));
        Assert.Equal("Publish failed", exception.Message);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to publish event TestEvent to RabbitMQ")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed record TestEvent
    {
        public string Message { get; init; } = string.Empty;
    }
}
