using Hipcall.Sdk.Deneme.Authentication;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Hipcall.Sdk.Deneme.Client;

public partial class HipcallClient
{
    /// <summary>
    /// Creates a new <see cref="HipcallClient"/> instance configured with a permanent static API token.
    /// No Dependency Injection setup is required.
    /// </summary>
    /// <param name="apiToken">Hipcall API token generated from the portal (Settings -> Developer -> API tokens).</param>
    /// <param name="httpClient">Optional custom <see cref="HttpClient"/>.</param>
    public static HipcallClient Create(string apiToken, HttpClient? httpClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiToken);

        var tokenProvider = new HipcallTokenProvider(apiToken);
        var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
        var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

        return new HipcallClient(requestAdapter);
    }

    /// <summary>
    /// Creates a new <see cref="HipcallClient"/> instance configured with a custom <see cref="IAccessTokenProvider"/>.
    /// </summary>
    public static HipcallClient Create(IAccessTokenProvider tokenProvider, HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);

        var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
        var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient);

        return new HipcallClient(requestAdapter);
    }
}
