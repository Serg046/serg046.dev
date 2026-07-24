using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class PullRequestCache(IMemoryCache cache, GitHubService gitHubService)
{
    private const string CacheKey = "PullRequests";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(90);

    public IReadOnlyList<GitHubPullRequest> Get() =>
        cache.TryGetValue(CacheKey, out IReadOnlyList<GitHubPullRequest>? pullRequests) ? pullRequests! : [];

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var pullRequests = await gitHubService.GetPullRequestsByStarsAsync(cancellationToken);
        cache.Set(CacheKey, pullRequests, Ttl);
    }
}
