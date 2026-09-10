using Microsoft.Extensions.Options;
using SeverityChecker.Worker;
using SeverityChecker.Worker.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<ScannerOptions>()
    .Bind(builder.Configuration.GetSection(ScannerOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<ScannerOptions>, ScannerOptionsValidator>();
builder.Services.AddSingleton<IScanCoordinator, ScanCoordinator>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
