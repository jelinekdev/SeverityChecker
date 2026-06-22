using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using SeverityChecker.Infrastructure.Osv;

namespace SeverityChecker.Infrastructure.Http;

public static class HttpClientExtensions
{
    public static IServiceCollection AddOsvHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient<OsvVulnerabilitySource>()
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }
}