namespace Client;

public record PullRequest(string Title, string Url, string Repo, string RepoUrl, int Stars, string State, DateTimeOffset CreatedAt);
