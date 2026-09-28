using SeverityChecker.Abstractions;
using SeverityChecker.Configuration;
using SeverityChecker.Scanners;
using SeverityChecker.Services;
using SeverityChecker.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options => options.ServiceName = "SeverityChecker");
builder.Services.AddSystemd();

builder.Services
    .AddOptions<ScanOptions>()
    .Bind(builder.Configuration.GetSection(ScanOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddTransient<IDependencyScanner, NuGetProjectScanner>();
builder.Services.AddTransient<IDependencyScanner, ConfiguredLibraryScanner>();

builder.Services.AddHttpClient<IVulnerabilityProvider, OsvVulnerabilityProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.osv.dev/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<IFindingStore, JsonFindingStore>();
builder.Services.AddHttpClient<INotificationSender, EmailNotificationSender>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped<VulnerabilityScanService>();

builder.Services.AddHostedService<SeverityCheckWorker>();

await builder.Build().RunAsync();
