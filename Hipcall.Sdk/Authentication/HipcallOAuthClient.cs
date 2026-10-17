using System.Net.Http.Json;
using Hipcall.Sdk.Deneme.Configuration;
using Microsoft.Extensions.Options;

namespace Hipcall.Sdk.Deneme.Authentication;

/// <summary>
/// Minimal client for the Hipcall OAuth2 token endpoint. Client credentials come from
/// <see cref="HipcallClientOptions"/>.
/// </summary>
public class HipcallOAuthClient
{
    /// <summary>Default token endpoint.</summary>
    public static readonly Uri TokenEndpoint = new("https://use.hipcall.com.tr/oauth/token");

    private readonly HttpClient _httpClient;
    private readonly IOptions<HipcallClientOptions> _options;

    public HipcallOAuthClient(HttpClient httpClient, IOptions<HipcallClientOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Exchanges an authorization <paramref name="code"/> for tokens
    /// (<c>grant_type=authorization_code</c>). The result is not stored automatically;
    /// pass it to <see cref="ITokenStore.SaveTokenAsync"/>.
    /// </summary>
    public Task<OAuthTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var o = _options.Value;

        return PostAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["client_id"] = o.ClientId ?? string.Empty,
            ["client_secret"] = o.ClientSecret ?? string.Empty,
            ["redirect_uri"] = o.RedirectUri ?? string.Empty,
        }, cancellationToken);
    }

    /// <summary>
    /// Gets a new token using a refresh token (<c>grant_type=refresh_token</c>).
    /// </summary>
    public Task<OAuthTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        var o = _options.Value;

        return PostAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = o.ClientId ?? string.Empty,
            ["client_secret"] = o.ClientSecret ?? string.Empty,
        }, cancellationToken);
    }

    private async Task<OAuthTokenResponse> PostAsync(
        Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Accept.Add(new("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new HttpRequestException(
                $"Hipcall token request failed with status {(int)response.StatusCode} ({response.StatusCode}): {body}",
                inner: null,
                statusCode: response.StatusCode);
        }

        var token = await response.Content
            .ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken)
            .ConfigureAwait(false);

        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            throw new InvalidOperationException("Hipcall token endpoint returned an empty or invalid token response.");
        }

        return token;
    }
}
