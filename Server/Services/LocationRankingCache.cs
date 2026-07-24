using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class LocationRankingCache(IMemoryCache cache, GitHubService gitHubService)
{
    private const string CacheKey = "LocationRankings";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(90);

    public IReadOnlyList<GitHubLocationRanking> Get() =>
        cache.TryGetValue(CacheKey, out IReadOnlyList<GitHubLocationRanking>? rankings) ? rankings! : [];

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var rankings = await gitHubService.GetLocationRankingsAsync(cancellationToken);
        cache.Set(CacheKey, rankings, Ttl);
    }
}
