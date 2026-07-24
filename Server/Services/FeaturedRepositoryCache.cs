using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class FeaturedRepositoryCache(IMemoryCache cache, GitHubService gitHubService)
{
    private const string CacheKey = "FeaturedRepositories";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(90);

    public IReadOnlyList<GitHubFeaturedRepository> Get() =>
        cache.TryGetValue(CacheKey, out IReadOnlyList<GitHubFeaturedRepository>? repos) ? repos! : [];

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var repos = await gitHubService.GetFeaturedRepositoriesAsync(cancellationToken);
        cache.Set(CacheKey, repos, Ttl);
    }
}
