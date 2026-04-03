using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MsProjects.Infrastructure;
using System.Text.Json;

namespace MsProjects.Tests.Infrastructure;

public sealed class GlobalExceptionHandlerShould
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _logger = new();

    private (GlobalExceptionHandler handler, DefaultHttpContext context) CreateHandler()
    {
        var handler = new GlobalExceptionHandler(_logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return (handler, context);
    }

    [Fact]
    public async Task TryHandleAsync_WithUnauthorizedAccessException_Returns403()
    {
        var (handler, context) = CreateHandler();
        var exception = new UnauthorizedAccessException("Not allowed");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task TryHandleAsync_WithArgumentException_Returns400()
    {
        var (handler, context) = CreateHandler();
        var exception = new ArgumentException("Bad argument");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithKeyNotFoundException_Returns404()
    {
        var (handler, context) = CreateHandler();
        var exception = new KeyNotFoundException("Not found");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_WithInvalidOperationException_Returns400()
    {
        var (handler, context) = CreateHandler();
        var exception = new InvalidOperationException("Invalid operation");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_Returns500()
    {
        var (handler, context) = CreateHandler();
        var exception = new Exception("Something broke");

        var result = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task TryHandleAsync_WritesProblemDetailsToResponse()
    {
        var (handler, context) = CreateHandler();
        context.Request.Path = "/api/test";
        var exception = new KeyNotFoundException("Item not found");

        await handler.TryHandleAsync(context, exception, CancellationToken.None);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        problemDetails.GetProperty("status").GetInt32().Should().Be(404);
        problemDetails.GetProperty("title").GetString().Should().Be("Not Found");
        problemDetails.GetProperty("detail").GetString().Should().Be("Item not found");
    }

    [Fact]
    public async Task TryHandleAsync_AlwaysReturnsTrue()
    {
        var (handler, context) = CreateHandler();

        var result = await handler.TryHandleAsync(context, new Exception(), CancellationToken.None);

        result.Should().BeTrue();
    }
}
