namespace Server.Services;

public record GitHubPullRequest(string Title, string Url, string Repo, string RepoUrl, int Stars, string State, DateTimeOffset CreatedAt);
