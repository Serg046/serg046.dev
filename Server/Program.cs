using Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddHealthChecks();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseWebAssemblyDebugging();
}
else
{
	app.UseExceptionHandler("/Error");
}

app.UseRequestLocalization("ru-RU");
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapHealthChecks("/ping");
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToPage("/_Host");
app.Run();
