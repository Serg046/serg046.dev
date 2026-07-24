using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class OrganizationCache(IMemoryCache cache, GitHubService gitHubService)
{
    private const string CacheKey = "Organizations";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(90);

    public IReadOnlyList<GitHubOrganization> Get() =>
        cache.TryGetValue(CacheKey, out IReadOnlyList<GitHubOrganization>? organizations) ? organizations! : [];

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await gitHubService.GetOrganizationsAsync(cancellationToken);
        cache.Set(CacheKey, organizations, Ttl);
    }
}
