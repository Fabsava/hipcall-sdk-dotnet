using Hipcall.Sdk.Authentication;
using Hipcall.Sdk.Client;
using Hipcall.Sdk.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Hipcall.Sdk.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>Name of the <see cref="HttpClient"/> used for Hipcall API calls.</summary>
    public const string ApiHttpClientName = "Hipcall.Api";

    /// <summary>
    /// Registers the Hipcall SDK: options, a default <see cref="InMemoryTokenStore"/>
    /// (only if no <see cref="ITokenStore"/> is registered yet), <see cref="HipcallOAuthClient"/>,
    /// <see cref="HipcallTokenProvider"/> and the Kiota-generated <see cref="HipcallClient"/>.
    /// </summary>
    /// <remarks>
    /// To use your own store, register it <b>before</b> calling this method:
    /// <c>services.AddSingleton&lt;ITokenStore, MyStore&gt;();</c>
    /// </remarks>
    public static IServiceCollection AddHipcallSdk(
        this IServiceCollection services,
        Action<HipcallClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.AddOptions<HipcallClientOptions>()
            .Configure(configureOptions)
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
                "HipcallClientOptions.ClientId and ClientSecret must be set.");

        // Skipped when the application already registered its own ITokenStore.
        services.TryAddSingleton<ITokenStore, InMemoryTokenStore>();

        // Typed client for the token endpoint.
        services.AddHttpClient<HipcallOAuthClient>();

        // HttpClient for API calls, with Kiota's default handlers (retry, redirect, compression...)
        // chained in front of the primary handler.
        services.AddHttpClient(ApiHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() =>
                KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(
                    new HttpClientHandler { AllowAutoRedirect = false },
                    KiotaClientFactory.CreateDefaultHandlers().ToArray())!);

        // Scoped, so a scoped ITokenStore (e.g. EF Core based) can be used safely.
        services.TryAddScoped<HipcallTokenProvider>();

        services.TryAddScoped<HipcallClient>(sp =>
        {
            var authProvider = new BaseBearerTokenAuthenticationProvider(
                sp.GetRequiredService<HipcallTokenProvider>());

            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(ApiHttpClientName);
            var adapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

            return new HipcallClient(adapter);
        });

        return services;
    }
}
