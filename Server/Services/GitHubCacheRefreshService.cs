namespace Server.Services;

// Startup population happens synchronously in Program.cs before app.Run(); this only
// handles the recurring refresh, so the timer's first tick (60 min) doesn't duplicate that.
public class GitHubCacheRefreshService(
    PullRequestCache pullRequestCache,
    OrganizationCache organizationCache,
    FeaturedRepositoryCache featuredRepositoryCache,
    LocationRankingCache locationRankingCache,
    ILogger<GitHubCacheRefreshService> logger) : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(RefreshInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await pullRequestCache.RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to refresh GitHub pull request cache");
            }

            try
            {
                await organizationCache.RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to refresh GitHub organization cache");
            }

            try
            {
                await featuredRepositoryCache.RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to refresh GitHub featured repository cache");
            }

            try
            {
                await locationRankingCache.RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to refresh GitHub location ranking cache");
            }
        }
    }
}
