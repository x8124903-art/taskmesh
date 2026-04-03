using MsProjects.Application.Models;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Domain.Services.Exceptions;
using MsProjects.Infrastructure.Repositories;
using System.Security.Cryptography;

namespace MsProjects.Domain.Services;

public sealed class ProjectInvitationService : IProjectInvitationService
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAuthorizationService _authService;
    private const int TOKEN_LENGTH = 32;
    private const int EXPIRATION_DAYS = 7;

    public ProjectInvitationService(
        IProjectInvitationRepository invitationRepository,
        IProjectMemberRepository memberRepository,
        IProjectRepository projectRepository,
        IProjectAuthorizationService authService)
    {
        _invitationRepository = invitationRepository;
        _memberRepository = memberRepository;
        _projectRepository = projectRepository;
        _authService = authService;
    }

    public async Task<ProjectInvitationModel> CreateInvitationAsync(
        InviteMemberRequest request,
        int invitedByUserId,
        CancellationToken cancellationToken = default,
        string? invitedByName = null)
    {
        var project = await _projectRepository.GetAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new ProjectNotFoundException(request.ProjectId);
        }

        if (!ProjectRoles.IsValidRole(request.Role))
        {
            throw new InvalidRoleException(request.Role);
        }

        if (await _memberRepository.ExistsMemberByProjectIdAndEmailAsync(
            request.ProjectId,
            request.Email,
            cancellationToken))
        {
            throw new DuplicateMemberException(request.ProjectId, request.Email);
        }

        if (await _invitationRepository.ExistsPendingByProjectAndEmailAsync(
            request.ProjectId, 
            request.Email, 
            cancellationToken))
        {
            throw new InvalidOperationException(
                $"A pending invitation already exists for '{request.Email}' in project {request.ProjectId}");
        }

        var token = GenerateSecureToken();
        
        var expiresAt = DateTime.UtcNow.AddDays(EXPIRATION_DAYS);

        return await _invitationRepository.CreateAsync(
            request.ProjectId,
            request.Email,
            request.Role,
            token,
            invitedByUserId,
            invitedByName,
            expiresAt,
            cancellationToken);
    }

    public async Task<ProjectInvitationModel?> GetInvitationByIdAsync(
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        return await _invitationRepository.GetByIdAsync(invitationId, cancellationToken);
    }

    public async Task<ProjectInvitationModel?> GetInvitationByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _invitationRepository.GetByTokenAsync(token, cancellationToken);
    }

    public async Task<IEnumerable<ProjectInvitationModel>> GetPendingInvitationsAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        await _invitationRepository.MarkExpiredInvitationsAsync(cancellationToken);
        
        return await _invitationRepository.GetPendingByProjectIdAsync(projectId, cancellationToken);
    }

    public async Task<IEnumerable<ProjectInvitationModel>> GetInvitationsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        await _invitationRepository.MarkExpiredInvitationsAsync(cancellationToken);
        
        return await _invitationRepository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task AcceptInvitationAsync(
        string token,
        int userId,
        CancellationToken cancellationToken = default,
        string? userName = null,
        string? email = null)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
        
        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        if (invitation.Status != "Pending")
        {
            throw new InvalidOperationException($"Invitation is {invitation.Status} and cannot be accepted");
        }

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            await _invitationRepository.MarkExpiredInvitationsAsync(cancellationToken);
            throw new InvalidOperationException("Invitation has expired");
        }

        if (await _memberRepository.ExistsByProjectIdAndUserIdAsync(invitation.ProjectId, userId, cancellationToken))
        {
            throw new DuplicateMemberException(invitation.ProjectId, userId);
        }

        await _memberRepository.AddMemberAsync(
            invitation.ProjectId,
            userId,
            ProjectRoles.GetNameFromId(invitation.Role),
            cancellationToken,
            userName,
            email ?? invitation.Email);

        await _invitationRepository.AcceptAsync(invitation.IdProjectInvitation, cancellationToken);
        
        await _authService.InvalidateUserProjectsCacheAsync(userId, cancellationToken);
    }

    public async Task RejectInvitationAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
        
        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        if (invitation.Status != "Pending")
        {
            throw new InvalidOperationException($"Invitation is {invitation.Status} and cannot be rejected");
        }

        await _invitationRepository.RejectAsync(invitation.IdProjectInvitation, cancellationToken);
    }

    public async Task DeleteInvitationAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _invitationRepository.GetByTokenAsync(token, cancellationToken);
        
        if (invitation == null)
        {
            throw new InvalidOperationException("Invitation not found");
        }

        if (invitation.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot delete invitation with status {invitation.Status}");
        }

        await _invitationRepository.DeleteAsync(invitation.IdProjectInvitation, cancellationToken);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[TOKEN_LENGTH];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
