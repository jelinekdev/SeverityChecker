using Microsoft.Extensions.Options;
using SeverityChecker.Worker;
using SeverityChecker.Worker.Configuration;
using SeverityChecker.Worker.Scanning;
using SeverityChecker.Worker.Scanning.NuGet;
using SeverityChecker.Worker.Scanning.Npm;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<ScannerOptions>()
    .Bind(builder.Configuration.GetSection(ScannerOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<VulnerabilityOptions>()
    .Bind(builder.Configuration.GetSection(VulnerabilityOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<NotificationOptions>()
    .Bind(builder.Configuration.GetSection(NotificationOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ScannerOptions>, ScannerOptionsValidator>();
builder.Services.AddSingleton<IValidateOptions<VulnerabilityOptions>, VulnerabilityOptionsValidator>();
builder.Services.AddSingleton<IValidateOptions<NotificationOptions>, NotificationOptionsValidator>();
builder.Services.AddSingleton<IScanCoordinator, ScanCoordinator>();
builder.Services
    .AddOptions<NpmScannerOptions>()
    .Bind(builder.Configuration.GetSection(NpmScannerOptions.SectionName));

builder.Services.AddSingleton<IDependencyScanner, NuGetDependencyScanner>();
builder.Services.AddSingleton<IDependencyScanner, NpmDependencyScanner>();
builder.Services.AddSingleton<IDependencyDiscoveryService, DependencyDiscoveryService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();




