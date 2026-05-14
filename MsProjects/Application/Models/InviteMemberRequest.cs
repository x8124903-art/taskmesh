namespace MsProjects.Application.Models;

public sealed record InviteMemberRequest(
    int ProjectId, 
    string Email,
    string Role);
