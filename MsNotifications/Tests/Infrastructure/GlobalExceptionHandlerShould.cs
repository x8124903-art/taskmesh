using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MsNotifications.Domain.Services.Exceptions;
using MsNotifications.Infrastructure;
using System.Text;
using System.Text.Json;

namespace MsNotifications.Tests.Infrastructure;

public sealed class GlobalExceptionHandlerShould
{
    [Fact]
    public async Task TryHandleAsync_Returns404_WhenNotificationNotFoundException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        var exception = new NotificationNotFoundException(1);

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var problemDetails = await GetProblemDetailsFromResponse(httpContext.Response);
        problemDetails.Status.Should().Be(404);
        problemDetails.Title.Should().Be("Notification not found");
    }

    [Fact]
    public async Task TryHandleAsync_Returns400_WhenDomainException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        var exception = new DomainException("Invalid operation");

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var problemDetails = await GetProblemDetailsFromResponse(httpContext.Response);
        problemDetails.Status.Should().Be(400);
        problemDetails.Title.Should().Be("Domain error");
        problemDetails.Detail.Should().Be("Invalid operation");
    }

    [Fact]
    public async Task TryHandleAsync_Returns403_WhenUnauthorizedAccessException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        var exception = new UnauthorizedAccessException("Access denied message");

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var problemDetails = await GetProblemDetailsFromResponse(httpContext.Response);
        problemDetails.Status.Should().Be(403);
        problemDetails.Title.Should().Be("Access denied");
        problemDetails.Detail.Should().Be("Access denied message");
    }

    [Fact]
    public async Task TryHandleAsync_Returns500_WhenGenericException()
    {
        // Arrange
        var logger = Mock.Of<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var problemDetails = await GetProblemDetailsFromResponse(httpContext.Response);
        problemDetails.Status.Should().Be(500);
        problemDetails.Title.Should().Be("Internal server error");
        problemDetails.Detail.Should().Be("Something went wrong");
    }

    [Fact]
    public async Task TryHandleAsync_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);
        var httpContext = CreateHttpContext();
        var exception = new Exception("Test error");

        // Act
        await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<ProblemDetails> GetProblemDetailsFromResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(response.Body, Encoding.UTF8).ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
