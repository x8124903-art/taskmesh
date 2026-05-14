using FluentAssertions;
using Microsoft.Extensions.Logging;
using MsAuth.Application.Models;
using MsAuth.Domain.Events;
using MsAuth.Domain.Services;
using MsAuth.Infrastructure.EventBus;
using MsAuth.Infrastructure.Repositories;

namespace MsAuth.Tests.Domain.Services;

public sealed class UserServiceShould
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly PasswordHasher _hasher;

    public UserServiceShould()
    {
        _repoMock = new Mock<IUserRepository>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _hasher = new PasswordHasher();
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsUser()
    {
        var expectedUser = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.GetByEmailAsync("test@demo.com", CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@demo.com");
        user.IdUser.Should().Be(1);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        _repoMock.Setup(x => x.GetByEmailAsync("nonexistent@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.GetByEmailAsync("nonexistent@demo.com", CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        var expectedUser = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.GetByIdAsync(1, CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.IdUser.Should().Be(1);
        user.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.GetByIdAsync(999, CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsUserWithHashedPassword()
    {
        var request = new RegisterRequest("newuser@demo.com", "John Doe", "Password123!");
        var expectedUser = new UserModel(1, "newuser@demo.com", "John Doe", "hashed", false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        _repoMock.Setup(x => x.AddAsync(
            It.Is<string>(e => e == request.Email),
            It.Is<string>(n => n == request.Name),
            It.Is<string>(h => h != "Password123!"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.RegisterAsync(request, CancellationToken.None);
        
        user.Should().NotBeNull();
        user.Email.Should().Be("newuser@demo.com");
        _repoMock.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_PublishesUserRegisteredEvent()
    {
        var request = new RegisterRequest("newuser@demo.com", "John Doe", "Password123!");
        var expectedUser = new UserModel(1, "newuser@demo.com", "John Doe", "hashed", false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        _repoMock.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        await service.RegisterAsync(request, CancellationToken.None);
        
        _eventBusMock.Verify(x => x.PublishAsync(
            It.Is<UserRegisteredEvent>(e => 
                e.UserId == expectedUser.IdUser &&
                e.Email == expectedUser.Email &&
                e.Name == expectedUser.Name),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_SucceedsEvenIfEventPublishingFails()
    {
        var request = new RegisterRequest("newuser@demo.com", "John Doe", "Password123!");
        var expectedUser = new UserModel(1, "newuser@demo.com", "John Doe", "hashed", false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        _repoMock.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        _eventBusMock.Setup(x => x.PublishAsync(It.IsAny<UserRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Event bus error"));
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.RegisterAsync(request, CancellationToken.None);
        
        user.Should().NotBeNull();
        user.IdUser.Should().Be(1);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to publish UserRegisteredEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithCorrectCredentials_ReturnsUser()
    {
        var password = "Password123!";
        var hashedPassword = _hasher.HashPassword(password);
        var expectedUser = new UserModel(1, "test@demo.com", "John", hashedPassword, false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.ValidateCredentialsAsync("test@demo.com", password, CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithIncorrectPassword_ReturnsNull()
    {
        var correctPassword = "Password123!";
        var hashedPassword = _hasher.HashPassword(correctPassword);
        var expectedUser = new UserModel(1, "test@demo.com", "John", hashedPassword, false, DateTime.UtcNow);
        
        _repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.ValidateCredentialsAsync("test@demo.com", "WrongPassword!", CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithNonExistentEmail_ReturnsNull()
    {
        _repoMock.Setup(x => x.GetByEmailAsync("nonexistent@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(_repoMock.Object, _hasher, _eventBusMock.Object, _loggerMock.Object);
        
        var user = await service.ValidateCredentialsAsync("nonexistent@demo.com", "AnyPassword!", CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var repoMock = new Mock<IUserRepository>();
        var hasher = new PasswordHasher();
        var existingUser = new UserModel(1, "existing@demo.com", "Existing User", "hash", false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.GetByEmailAsync("existing@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        
        var eventBusMock = new Mock<IEventBus>();
        var loggerMock = new Mock<ILogger<UserService>>();
        var service = new UserService(repoMock.Object, hasher, eventBusMock.Object, loggerMock.Object);
        var request = new RegisterRequest("existing@demo.com", "New User", "Password123!");
        
        Func<Task> act = () => service.RegisterAsync(request, CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*existing@demo.com*already exists*");
    }
}
