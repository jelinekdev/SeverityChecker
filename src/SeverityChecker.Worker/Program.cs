using Microsoft.Extensions.Options;
using SeverityChecker.Worker;
using SeverityChecker.Worker.Configuration;

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
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
