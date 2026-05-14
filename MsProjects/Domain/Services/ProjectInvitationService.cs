using MsProjects.Application.Models;
using MsProjects.Domain.Events;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Domain.Services.Exceptions;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Infrastructure.HttpClients;
using MsProjects.Infrastructure.Repositories;
using System.Security.Cryptography;

namespace MsProjects.Domain.Services;

public sealed class ProjectInvitationService : IProjectInvitationService
{
    private readonly IProjectInvitationRepository _invitationRepository;
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAuthorizationService _authService;
    private readonly IEventBus _eventBus;
    private readonly IAuthHttpClient _authHttpClient;
    private readonly ILogger<ProjectInvitationService> _logger;
    private const int TOKEN_LENGTH = 32;
    private const int EXPIRATION_DAYS = 7;

    public ProjectInvitationService(
        IProjectInvitationRepository invitationRepository,
        IProjectMemberRepository memberRepository,
        IProjectRepository projectRepository,
        IProjectAuthorizationService authService,
        IEventBus eventBus,
        IAuthHttpClient authHttpClient,
        ILogger<ProjectInvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _memberRepository = memberRepository;
        _projectRepository = projectRepository;
        _authService = authService;
        _eventBus = eventBus;
        _authHttpClient = authHttpClient;
        _logger = logger;
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

        var invitation = await _invitationRepository.CreateAsync(
            request.ProjectId,
            request.Email,
            request.Role,
            token,
            invitedByUserId,
            invitedByName,
            expiresAt,
            cancellationToken);

        try
        {
            var invitedUserId = await _memberRepository.GetUserIdByEmailAsync(request.Email, cancellationToken)
                ?? await _authHttpClient.GetUserIdByEmailAsync(request.Email, cancellationToken);

            var evt = new MemberInvitedEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredAt: DateTime.UtcNow,
                ProjectId: project.IdProject,
                ProjectName: project.Name,
                InvitedEmail: request.Email,
                RoleName: request.Role,
                InvitedByUserId: invitedByUserId,
                InvitedUserId: invitedUserId
            );
            await _eventBus.PublishAsync(evt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberInvitedEvent for project {ProjectId}", request.ProjectId);
        }

        return invitation;
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

        var project = await _projectRepository.GetAsync(invitation.ProjectId, cancellationToken);

        await _memberRepository.AddMemberAsync(
            invitation.ProjectId,
            userId,
            ProjectRoles.GetNameFromId(invitation.Role),
            cancellationToken,
            userName,
            email ?? invitation.Email);

        await _invitationRepository.AcceptAsync(invitation.IdProjectInvitation, cancellationToken);
        
        await _authService.InvalidateUserProjectsCacheAsync(userId, cancellationToken);

        try
        {
            var evt = new MemberJoinedEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredAt: DateTime.UtcNow,
                ProjectId: invitation.ProjectId,
                ProjectName: project?.Name ?? "Unknown",
                UserId: userId,
                RoleName: ProjectRoles.GetNameFromId(invitation.Role)
            );
            await _eventBus.PublishAsync(evt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberJoinedEvent for project {ProjectId}", invitation.ProjectId);
        }
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
