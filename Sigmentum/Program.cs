using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Sigmentum.Components;
using Sigmentum.Services;
using Sigmentum.Background;
using Sigmentum.Endpoints;
using Sigmentum.Infrastructure.Persistence.Configurations;
using Sigmentum.Infrastructure.Persistence.DbContext;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // <-- this is key!

var connectionString = builder.Configuration.GetConnectionString("DBConnection");

builder.Services.AddDbContext<SigmentumDbContext>(options => 
    options.UseNpgsql(connectionString));
builder.Services.AddDbContextFactory<LogDbContext>(options =>
    options.UseNpgsql(connectionString));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Logging.ClearProviders();

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Id", new UuidColumnWriter() },
    { "Message", new RenderedMessageColumnWriter() },
    { "MessageTemplate", new MessageTemplateColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "TimeStamp", new TimestampColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "LogEvent", new LogEventSerializedColumnWriter() }
};

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    // .WriteTo.PostgreSQL(
    //     connectionString: connectionString,
    //     tableName: "Logs",
    //     columnOptions: columnWriters,
    //     needAutoCreateTable: true,
    //     restrictedToMinimumLevel: LogEventLevel.Information,
    //     formatProvider: CultureInfo.InvariantCulture,
    //     // 👇 This filters the sink by property
    //     logEventFilter: logEvent => 
    //         logEvent.Properties.TryGetValue("LogToDb", out var val) &&
    //         val.ToString().ToLowerInvariant() == "true"
    // )
    .WriteTo.Logger(cfg => cfg
        .Filter.ByIncludingOnly(logEvent =>
            logEvent.Level >= LogEventLevel.Warning ||
            (logEvent.Properties.TryGetValue("LogToDb", out var v) &&
             v.ToString().Equals("true", StringComparison.InvariantCultureIgnoreCase)))
        .WriteTo.PostgreSQL(
            connectionString: connectionString,
            tableName: "Logs",
            columnOptions: columnWriters,
            needAutoCreateTable: true,
            restrictedToMinimumLevel: LogEventLevel.Information
        )
    )
    .CreateLogger();

// builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning); // adjust here
// builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);

builder.Host.UseSerilog();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Razor + Blazor support
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpClient
builder.Services.AddScoped(sp =>
{
    var navManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navManager.BaseUri) };
});

// Register services
builder.Services.AddSingleton<BinanceDataFetcher>();
builder.Services.AddSingleton<TwelveDataFetcher>();
builder.Services.AddSingleton<EvaluationService>();
builder.Services.AddSingleton<SignalService>();
builder.Services.AddSingleton<ConfigService>();

// Register background services
builder.Services.AddHostedService<EvaluationBackgroundService>();
builder.Services.AddHostedService<BinancePollingService>();
builder.Services.AddHostedService<TwelvePollingService>();

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

app.MapGet("/api/live-signals", () => Results.Ok(CacheService.LatestSignals));
app.MapEvaluationMetrics();
app.MapScanResults();

app.Run();