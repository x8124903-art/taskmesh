using FluentAssertions;
using MsProjects.Domain.Services.Exceptions;

namespace MsProjects.Tests.Domain.Services.Exceptions;

public sealed class DomainExceptionsShould
{
    [Fact]
    public void DuplicateInvitationException_WithProjectIdAndEmail_CreatesWithCorrectMessage()
    {
        var exception = new DuplicateInvitationException(1, "test@demo.com");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("test@demo.com");
        exception.Message.Should().Contain("1");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvitationExpiredException_WithExpirationDate_CreatesWithCorrectMessage()
    {
        var expirationDate = new DateTime(2024, 12, 31, 23, 59, 59);
        
        var exception = new InvitationExpiredException(expirationDate);
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("31/12/2024");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvitationNotFoundException_WithToken_CreatesWithCorrectMessage()
    {
        var exception = new InvitationNotFoundException("token123");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("token123");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvitationAlreadyProcessedException_WithStatus_CreatesWithCorrectMessage()
    {
        var exception = new InvitationAlreadyProcessedException("Accepted");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Accepted");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void DuplicateMemberException_WithProjectIdAndUserId_CreatesWithCorrectMessage()
    {
        var exception = new DuplicateMemberException(1, 100);
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("100");
        exception.Message.Should().Contain("1");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void DuplicateMemberException_WithEmail_CreatesWithCorrectMessage()
    {
        var exception = new DuplicateMemberException(1, "member@test.com");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("member@test.com");
        exception.Message.Should().Contain("1");
        exception.ProjectId.Should().Be(1);
        exception.Email.Should().Be("member@test.com");
        exception.UserId.Should().BeNull();
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvalidRoleException_WithRole_CreatesWithCorrectMessage()
    {
        var exception = new InvalidRoleException("InvalidRole");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("InvalidRole");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ProjectNotFoundException_WithId_CreatesWithCorrectMessage()
    {
        var exception = new ProjectNotFoundException(999);
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("999");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void UnauthorizedProjectAccessException_WithUserIdAndProjectId_CreatesWithCorrectMessage()
    {
        var exception = new UnauthorizedProjectAccessException(100, 1, "delete");
        
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("100");
        exception.Message.Should().Contain("1");
        exception.Message.Should().Contain("delete");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void InvitationNotFoundException_WithId_CreatesWithCorrectMessage()
    {
        var exception = new InvitationNotFoundException(42);
        exception.Message.Should().Contain("42");
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ProjectNotFoundException_ExposesProjectId()
    {
        var exception = new ProjectNotFoundException(99);
        exception.ProjectId.Should().Be(99);
    }

    [Fact]
    public void UnauthorizedProjectAccessException_ExposesProperties()
    {
        var exception = new UnauthorizedProjectAccessException(10, 20, "edit");
        exception.UserId.Should().Be(10);
        exception.ProjectId.Should().Be(20);
        exception.Action.Should().Be("edit");
    }

    [Fact]
    public void DuplicateMemberException_ExposesProperties()
    {
        var exception = new DuplicateMemberException(5, 10);
        exception.ProjectId.Should().Be(5);
        exception.UserId.Should().Be(10);
    }

    [Fact]
    public void InvalidRoleException_ExposesRole()
    {
        var exception = new InvalidRoleException("BadRole");
        exception.Role.Should().Be("BadRole");
    }

    private sealed class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
        public TestDomainException(string message, Exception innerException) : base(message, innerException) { }
    }

    [Fact]
    public void DomainException_WithInnerException_PreservesInnerException()
    {
        var inner = new InvalidOperationException("inner error");
        var exception = new TestDomainException("outer error", inner);
        exception.Message.Should().Be("outer error");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void DomainException_WithMessage_SetsMessage()
    {
        var exception = new TestDomainException("test message");
        exception.Message.Should().Be("test message");
        exception.InnerException.Should().BeNull();
    }
}
