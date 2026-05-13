using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MsTasks.Domain.Exceptions;
using MsTasks.Infrastructure;

namespace MsTasks.Tests.Infrastructure;

public sealed class GlobalExceptionHandlerShould
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerShould()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task ReturnNotFound_WhenTaskNotFoundException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new TaskNotFoundException(1);

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task ReturnConflict_WhenConcurrencyException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new ConcurrencyException();

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task ReturnBadRequest_WhenDomainException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new DomainException("Invalid operation");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task ReturnForbidden_WhenUnauthorizedAccessException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new UnauthorizedAccessException("No permission");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task ReturnInternalServerError_WhenGenericException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new Exception("Unexpected error");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task ReturnBadRequest_WhenArgumentException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new ArgumentException("Invalid argument");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task LogError_WhenExceptionOccurs()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
