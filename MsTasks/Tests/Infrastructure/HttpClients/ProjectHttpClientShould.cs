using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MsTasks.Infrastructure.HttpClients;

namespace MsTasks.Tests.Infrastructure.HttpClients;

public sealed class ProjectHttpClientShould
{
    private readonly Mock<ILogger<ProjectHttpClient>> _loggerMock;

    public ProjectHttpClientShould()
    {
        _loggerMock = new Mock<ILogger<ProjectHttpClient>>();
    }

    [Fact]
    public async Task ReturnRole_WhenUserIsMember()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;
        const string EXPECTED_ROLE = "Owner";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.PathAndQuery.Contains($"projects/{GIVEN_PROJECT_ID}/members/{GIVEN_USER_ID}/role")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"{{\"role\":\"{EXPECTED_ROLE}\"}}")
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:44311")
        };

        var client = new ProjectHttpClient(httpClient, _loggerMock.Object);

        // Act
        var result = await client.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().Be(EXPECTED_ROLE);
    }

    [Fact]
    public async Task ReturnNull_WhenUserIsNotMember()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 999;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:44311")
        };

        var client = new ProjectHttpClient(httpClient, _loggerMock.Object);

        // Act
        var result = await client.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ThrowHttpRequestException_WhenMsProjectsUnavailable()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:44311")
        };

        var client = new ProjectHttpClient(httpClient, _loggerMock.Object);

        // Act
        Func<Task> act = async () => await client.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ThrowHttpRequestException_WhenInternalServerError()
    {
        // Arrange
        const int GIVEN_PROJECT_ID = 1;
        const int GIVEN_USER_ID = 10;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:44311")
        };

        var client = new ProjectHttpClient(httpClient, _loggerMock.Object);

        // Act
        Func<Task> act = async () => await client.GetUserRoleInProjectAsync(GIVEN_PROJECT_ID, GIVEN_USER_ID, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
