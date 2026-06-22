using SeverityChecker;
using SeverityChecker.Core.Interfaces;
using SeverityChecker.Core.Options;
using SeverityChecker.Infrastructure.Http;
using SeverityChecker.Infrastructure.Mail;
using SeverityChecker.Infrastructure.Osv;
using SeverityChecker.Infrastructure.Parsers;
using SeverityChecker.Infrastructure.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ScanOptions>(
    builder.Configuration.GetSection(ScanOptions.SectionName));

builder.Services.Configure<MailOptions>(
    builder.Configuration.GetSection(MailOptions.SectionName));

builder.Services.AddSingleton<NuGetPackageParser>();
builder.Services.AddOsvHttpClient();
builder.Services.AddTransient<IVulnerabilitySource, OsvVulnerabilitySource>();
builder.Services.AddTransient<IScanService, ScanService>();
builder.Services.AddTransient<INotificationService, MailNotificationService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();