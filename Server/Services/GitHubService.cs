using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Server.Services;

public class GitHubService(IHttpClientFactory httpClientFactory)
{
    private const string Username = "Serg046";

    private static readonly string[] RankedLocations = ["Rostov-on-Don", "Russia"];

    public async Task<IReadOnlyList<GitHubPullRequest>> GetPullRequestsByStarsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("GitHub");

        var items = new List<SearchIssueItem>();
        var page = 1;
        while (true)
        {
            var query = Uri.EscapeDataString($"type:pr author:{Username}");
            var response = await client.GetFromJsonAsync<SearchIssuesResponse>(
                $"search/issues?q={query}&per_page=100&page={page}", cancellationToken);
            if (response?.Items is not { Count: > 0 } pageItems) break;
            items.AddRange(pageItems);
            if (pageItems.Count < 100) break;
            page++;
        }

        var repoUrls = items.Select(i => i.RepositoryUrl).Distinct().ToList();
        var starsByRepo = await GetStarsByRepoAsync(client, repoUrls, cancellationToken);

        return items
            .Select(i =>
            {
                var (fullName, stars) = starsByRepo.TryGetValue(i.RepositoryUrl, out var repo)
                    ? repo
                    : (FallbackRepoName(i.RepositoryUrl), 0);
                return new GitHubPullRequest(i.Title, i.HtmlUrl, fullName, $"https://github.com/{fullName}", stars, i.State, i.CreatedAt);
            })
            .Where(pr => !IsOwnRepo(pr.Repo) && pr.Stars > 10)
            .OrderByDescending(pr => pr.Stars)
            .ThenByDescending(pr => pr.CreatedAt)
            .ToList();
    }

    public async Task<IReadOnlyList<GitHubFeaturedRepository>> GetFeaturedRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("GitHub");

        var repos = await client.GetFromJsonAsync<List<RepoListItem>>(
            $"users/{Username}/repos?type=owner&per_page=100", cancellationToken) ?? [];

        return repos
            .Where(r => !r.Fork && r.StargazersCount > 10)
            .OrderByDescending(r => r.StargazersCount)
            .Select(r => new GitHubFeaturedRepository(r.Name, r.StargazersCount, r.HtmlUrl, r.Description))
            .ToList();
    }

    // Ranks Serg046 by follower count among GitHub users whose profile location matches each entry
    // in RankedLocations, using only search "total_count" (no per-user enumeration needed):
    // rank = count of users in that location with more followers, + 1.
    public async Task<IReadOnlyList<GitHubLocationRanking>> GetLocationRankingsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("GitHub");

        var profile = await client.GetFromJsonAsync<UserProfile>($"users/{Username}", cancellationToken);
        var followers = profile?.Followers ?? 0;

        var result = new List<GitHubLocationRanking>();
        foreach (var location in RankedLocations)
        {
            var total = await GetSearchTotalCountAsync(client, $"location:\"{location}\"", cancellationToken);
            var ahead = await GetSearchTotalCountAsync(client, $"location:\"{location}\" followers:>{followers}", cancellationToken);
            result.Add(new GitHubLocationRanking(location, ahead + 1, total));
        }

        return result;
    }

    private static async Task<int> GetSearchTotalCountAsync(HttpClient client, string query, CancellationToken cancellationToken)
    {
        var response = await client.GetFromJsonAsync<SearchUsersResponse>(
            $"search/users?q={Uri.EscapeDataString(query)}&per_page=1", cancellationToken);
        return response?.TotalCount ?? 0;
    }

    public async Task<IReadOnlyList<GitHubOrganization>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("GitHub");

        var orgs = await client.GetFromJsonAsync<List<OrgListItem>>($"users/{Username}/orgs", cancellationToken) ?? [];

        var result = new List<GitHubOrganization>();
        foreach (var org in orgs)
        {
            var details = await client.GetFromJsonAsync<OrgDetails>($"orgs/{org.Login}", cancellationToken);
            result.Add(new GitHubOrganization(
                org.Login,
                details?.Name ?? org.Login,
                details?.Description ?? org.Description,
                org.AvatarUrl,
                $"https://github.com/{org.Login}"));
        }

        return result;
    }

    // Fetches all repos' star counts in a single GraphQL request instead of one REST call per repo.
    // Requires GitHubToken to be configured; the GraphQL API has no unauthenticated quota.
    private static async Task<Dictionary<string, (string FullName, int Stars)>> GetStarsByRepoAsync(
        HttpClient client, IReadOnlyList<string> repoUrls, CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, (string FullName, int Stars)>();
        if (repoUrls.Count == 0)
        {
            return result;
        }

        var aliasedRepos = repoUrls
            .Select((url, index) => (Alias: $"repo{index}", Url: url, OwnerRepo: FallbackRepoName(url).Split('/', 2)))
            .Where(r => r.OwnerRepo.Length == 2)
            .ToList();

        var query = "{" + string.Join(' ', aliasedRepos.Select(r =>
            $"{r.Alias}: repository(owner: \"{r.OwnerRepo[0]}\", name: \"{r.OwnerRepo[1]}\") {{ nameWithOwner stargazerCount }}")) + "}";

        try
        {
            var response = await client.PostAsJsonAsync("graphql", new { query }, cancellationToken);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<GraphQlResponse>(cancellationToken);

            foreach (var r in aliasedRepos)
            {
                if (payload?.Data?.GetValueOrDefault(r.Alias) is { } repo)
                {
                    result[r.Url] = (repo.NameWithOwner, repo.StargazerCount);
                }
            }
        }
        catch (HttpRequestException)
        {
            // GitHubToken missing/invalid or GraphQL rate limit hit; fall back to 0 stars for every repo below.
        }

        return result;
    }

    private static bool IsOwnRepo(string fullName) =>
        fullName.Split('/', 2) is [var owner, _] && string.Equals(owner, Username, StringComparison.OrdinalIgnoreCase);

    private static string FallbackRepoName(string repositoryUrl) =>
        repositoryUrl.Split("/repos/", 2) is [_, var ownerRepo] ? ownerRepo : repositoryUrl;

    private record SearchIssuesResponse([property: JsonPropertyName("items")] List<SearchIssueItem> Items);

    private record SearchIssueItem(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("repository_url")] string RepositoryUrl,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt);

    private record GraphQlResponse([property: JsonPropertyName("data")] Dictionary<string, GraphQlRepo?>? Data);

    private record GraphQlRepo(
        [property: JsonPropertyName("nameWithOwner")] string NameWithOwner,
        [property: JsonPropertyName("stargazerCount")] int StargazerCount);

    private record OrgListItem(
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("avatar_url")] string AvatarUrl,
        [property: JsonPropertyName("description")] string? Description);

    private record OrgDetails(
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("description")] string? Description);

    private record RepoListItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("stargazers_count")] int StargazersCount,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("fork")] bool Fork,
        [property: JsonPropertyName("description")] string? Description);

    private record UserProfile([property: JsonPropertyName("followers")] int Followers);

    private record SearchUsersResponse([property: JsonPropertyName("total_count")] int TotalCount);
}
