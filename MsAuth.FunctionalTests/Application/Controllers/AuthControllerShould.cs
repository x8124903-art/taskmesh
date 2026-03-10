using Newtonsoft.Json;

namespace MsAuth.FunctionalTests.Application.Controllers;

[Collection(nameof(ServerFixtureCollection))]
public class AuthControllerShould(ServerFixture fixture)
{
    private readonly ServerFixture _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    
    [Fact]
    public async Task Register_ReturnsCreated_WhenValidRequest()
    {
        var request = new
        {
            email = $"newuser{Guid.NewGuid()}@taskmesh.com",
            name = "New User",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("email");
        stringResult.Should().Contain(request.email);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailExists()
    {
        var request = new
        {
            email = "testuser@taskmesh.com",
            name = "Duplicate User",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenValidCredentials()
    {
        var request = new
        {
            email = "testuser@taskmesh.com",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("accessToken");
        stringResult.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalidPassword()
    {
        var request = new
        {
            email = "testuser@taskmesh.com",
            password = "WrongPassword123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
    {
        var request = new
        {
            email = "nonexistent@taskmesh.com",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenValidToken()
    {
        var loginRequest = new
        {
            email = "testuser@taskmesh.com",
            password = "Test123!"
        };
        var loginContent = new StringContent(
            JsonConvert.SerializeObject(loginRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");
        
        var loginResponse = await _fixture.CreateClient().PostAsync("auth/login", loginContent);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        dynamic loginData = JsonConvert.DeserializeObject(loginResult)!;
        
        var refreshRequest = new
        {
            refreshToken = (string)loginData.refreshToken
        };
        var refreshContent = new StringContent(
            JsonConvert.SerializeObject(refreshRequest), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/refresh", refreshContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("accessToken");
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailInvalid()
    {
        var request = new
        {
            email = "invalid-email",
            name = "Test User",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenPasswordTooShort()
    {
        var request = new
        {
            email = $"user{Guid.NewGuid()}@taskmesh.com",
            name = "Test User",
            password = "123"
        };
        var content = new StringContent(
            JsonConvert.SerializeObject(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        var response = await _fixture.CreateClient().PostAsync("auth/register", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
