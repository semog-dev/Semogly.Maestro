using Microsoft.Extensions.DependencyInjection;
using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.Http;

public static class HttpActivitiesServiceCollectionExtensions
{
    /// <summary>Registers HttpRequestActivityHandler under HttpActivityTypes.Request, backed by an IHttpClientFactory-managed typed HttpClient. Returns the IHttpClientBuilder so callers can chain further HttpClient configuration.</summary>
    public static IHttpClientBuilder AddHttpActivities(this IServiceCollection services)
    {
        services.AddActivityHandler(HttpActivityTypes.Request, sp => sp.GetRequiredService<HttpRequestActivityHandler>());
        return services.AddHttpClient<HttpRequestActivityHandler>();
    }
}
