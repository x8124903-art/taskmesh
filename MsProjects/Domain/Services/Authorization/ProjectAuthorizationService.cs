using Microsoft.Extensions.Caching.Distributed;
using MsProjects.Infrastructure.Repositories;
using System.Text.Json;

namespace MsProjects.Domain.Services.Authorization;

/// <summary>
/// Implementation of project authorization service with Redis caching.
/// </summary>
public sealed class ProjectAuthorizationService : IProjectAuthorizationService
{
    private readonly IProjectMemberRepository _memberRepository;
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public ProjectAuthorizationService(
        IProjectMemberRepository memberRepository,
        IDistributedCache cache)
    {
        _memberRepository = memberRepository;
        _cache = cache;
    }

    public async Task<bool> IsProjectMemberAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        var role = await GetUserProjectRoleAsync(userId, projectId, cancellationToken);
        return role != null;
    }

    public async Task<bool> HasProjectRoleAsync(int userId, int projectId, CancellationToken cancellationToken, params string[] allowedRoles)
    {
        var userRole = await GetUserProjectRoleAsync(userId, projectId, cancellationToken);
        return userRole != null && allowedRoles.Contains(userRole);
    }

    public async Task<bool> IsProjectOwnerAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        var role = await GetUserProjectRoleAsync(userId, projectId, cancellationToken);
        return role == ProjectRoles.Owner;
    }

    public async Task<string?> GetUserProjectRoleAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"project_role:{userId}:{projectId}";
        
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        var role = await _memberRepository.GetUserRoleInProjectAsync(userId, projectId, cancellationToken);

        if (role != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };
            await _cache.SetStringAsync(cacheKey, role, options, cancellationToken);
        }

        return role;
    }

    public async Task<List<int>> GetUserProjectIdsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_projects:{userId}";

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<List<int>>(cached) ?? new List<int>();
        }

        var projectIds = await _memberRepository.GetProjectIdsByUserIdAsync(userId, cancellationToken);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration
        };
        var json = JsonSerializer.Serialize(projectIds);
        await _cache.SetStringAsync(cacheKey, json, options, cancellationToken);

        return projectIds;
    }

    public async Task InvalidateUserProjectsCacheAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user_projects:{userId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }

    public async Task InvalidateUserProjectRoleCacheAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"project_role:{userId}:{projectId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }
}
