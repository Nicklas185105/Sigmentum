using Microsoft.AspNetCore.Components;
using Sigmentum.Components;
using Sigmentum.Services;
using Sigmentum.Background;
using Sigmentum.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // <-- this is key!


// Add Razor + Blazor support
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient
builder.Services.AddScoped(sp =>
{
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
});

// Register background services
builder.Services.AddHostedService<EvaluationBackgroundService>();
builder.Services.AddHostedService<SignalPollingService>();

// Register any singletons like SignalCache if needed
// builder.Services.AddSingleton<SignalCache>(); <-- optional if needed

var app = builder.Build();

// Usual pipeline setup
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapGet("/api/live-signals", () => Results.Ok(SignalCache.LatestSignals));
app.MapEvaluationMetrics();
app.MapScanResults();

app.Run();