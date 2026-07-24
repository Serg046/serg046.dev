using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Server.Components;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Lets Home.razor's @inject HttpClient resolve during the server-side prerender pass;
// the WebAssembly runtime uses its own HttpClient registration (Client/Program.cs) after hydration.
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

builder.Services.AddHttpClient("GitHub", (sp, client) =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("serg046.is-a.dev");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

    var token = sp.GetRequiredService<IConfiguration>()["GitHubToken"];
    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
});
builder.Services.AddSingleton<GitHubService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<PullRequestCache>();
builder.Services.AddSingleton<OrganizationCache>();
builder.Services.AddSingleton<FeaturedRepositoryCache>();
builder.Services.AddSingleton<LocationRankingCache>();
builder.Services.AddHostedService<GitHubCacheRefreshService>();

var app = builder.Build();

try
{
    await app.Services.GetRequiredService<PullRequestCache>().RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to warm GitHub pull request cache at startup");
}

try
{
    await app.Services.GetRequiredService<OrganizationCache>().RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to warm GitHub organization cache at startup");
}

try
{
    await app.Services.GetRequiredService<FeaturedRepositoryCache>().RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to warm GitHub featured repository cache at startup");
}

try
{
    await app.Services.GetRequiredService<LocationRankingCache>().RefreshAsync();
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to warm GitHub location ranking cache at startup");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapGet("/api/prs", (PullRequestCache cache) => cache.Get());
app.MapGet("/api/orgs", (OrganizationCache cache) => cache.Get());
app.MapGet("/api/featured-repos", (FeaturedRepositoryCache cache) => cache.Get());
app.MapGet("/api/location-rankings", (LocationRankingCache cache) => cache.Get());

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

app.Run();