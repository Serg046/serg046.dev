namespace Server.Services;

public record GitHubOrganization(string Login, string Name, string? Description, string AvatarUrl, string Url);
