using FluentAssertions;
using MsAuth.Application.Models;
using MsAuth.Domain.Services;
using MsAuth.Infrastructure.Repositories;

namespace MsAuth.Tests.Domain.Services;

public sealed class UserServiceShould
{
    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsUser()
    {
        var repoMock = new Mock<IUserRepository>();
        var expectedUser = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(repoMock.Object, new PasswordHasher());
        
        var user = await service.GetByEmailAsync("test@demo.com", CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@demo.com");
        user.IdUser.Should().Be(1);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ReturnsNull()
    {
        var repoMock = new Mock<IUserRepository>();
        
        repoMock.Setup(x => x.GetByEmailAsync("nonexistent@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(repoMock.Object, new PasswordHasher());
        
        var user = await service.GetByEmailAsync("nonexistent@demo.com", CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        var repoMock = new Mock<IUserRepository>();
        var expectedUser = new UserModel(1, "test@demo.com", "John", "hash", false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(repoMock.Object, new PasswordHasher());
        
        var user = await service.GetByIdAsync(1, CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.IdUser.Should().Be(1);
        user.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        var repoMock = new Mock<IUserRepository>();
        
        repoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(repoMock.Object, new PasswordHasher());
        
        var user = await service.GetByIdAsync(999, CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsUserWithHashedPassword()
    {
        var repoMock = new Mock<IUserRepository>();
        var hasher = new PasswordHasher();
        var request = new RegisterRequest("newuser@demo.com", "John Doe", "Password123!");
        var expectedUser = new UserModel(1, "newuser@demo.com", "John", "hashed", false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.AddAsync(
            It.Is<string>(e => e == request.Email),
            It.Is<string>(n => n == request.Name),
            It.Is<string>(h => h != "Password123!"),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(repoMock.Object, hasher);
        
        var user = await service.RegisterAsync(request, CancellationToken.None);
        
        user.Should().NotBeNull();
        user.Email.Should().Be("newuser@demo.com");
        repoMock.Verify(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithCorrectCredentials_ReturnsUser()
    {
        var repoMock = new Mock<IUserRepository>();
        var hasher = new PasswordHasher();
        var password = "Password123!";
        var hashedPassword = hasher.HashPassword(password);
        var expectedUser = new UserModel(1, "test@demo.com", "John", hashedPassword, false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(repoMock.Object, hasher);
        
        var user = await service.ValidateCredentialsAsync("test@demo.com", password, CancellationToken.None);
        
        user.Should().NotBeNull();
        user!.Email.Should().Be("test@demo.com");
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithIncorrectPassword_ReturnsNull()
    {
        var repoMock = new Mock<IUserRepository>();
        var hasher = new PasswordHasher();
        var correctPassword = "Password123!";
        var hashedPassword = hasher.HashPassword(correctPassword);
        var expectedUser = new UserModel(1, "test@demo.com", "John", hashedPassword, false, DateTime.UtcNow);
        
        repoMock.Setup(x => x.GetByEmailAsync("test@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);
        
        var service = new UserService(repoMock.Object, hasher);
        
        var user = await service.ValidateCredentialsAsync("test@demo.com", "WrongPassword!", CancellationToken.None);
        
        user.Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithNonExistentEmail_ReturnsNull()
    {
        var repoMock = new Mock<IUserRepository>();
        var hasher = new PasswordHasher();
        
        repoMock.Setup(x => x.GetByEmailAsync("nonexistent@demo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserModel?)null);
        
        var service = new UserService(repoMock.Object, hasher);
        
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
        
        var service = new UserService(repoMock.Object, hasher);
        var request = new RegisterRequest("existing@demo.com", "New User", "Password123!");
        
        Func<Task> act = () => service.RegisterAsync(request, CancellationToken.None);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*existing@demo.com*already exists*");
    }
}
