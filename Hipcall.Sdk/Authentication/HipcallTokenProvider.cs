using Hipcall.Sdk.Deneme.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Hipcall.Sdk.Deneme.Authentication;

/// <summary>
/// Kiota <see cref="IAccessTokenProvider"/> that supports both static permanent API Tokens
/// and automatic OAuth2 token management via <see cref="ITokenStore"/> and <see cref="HipcallOAuthClient"/>.
/// </summary>
public class HipcallTokenProvider : IAccessTokenProvider
{
    private static readonly string[] DefaultAllowedHosts = ["use.hipcall.com", "use.hipcall.com.tr"];

    private readonly string? _staticApiToken;
    private readonly IOptions<HipcallClientOptions>? _options;
    private readonly ITokenStore? _tokenStore;
    private readonly HipcallOAuthClient? _oauthClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// Initializes a provider with a static permanent API token.
    /// </summary>
    public HipcallTokenProvider(string apiToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiToken);
        _staticApiToken = apiToken;
        AllowedHostsValidator = new AllowedHostsValidator(DefaultAllowedHosts);
    }

    /// <summary>
    /// Initializes a provider for OAuth2 authorization code / refresh token flow.
    /// </summary>
    public HipcallTokenProvider(ITokenStore tokenStore, HipcallOAuthClient oauthClient)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _oauthClient = oauthClient ?? throw new ArgumentNullException(nameof(oauthClient));
        AllowedHostsValidator = new AllowedHostsValidator(DefaultAllowedHosts);
    }

    /// <summary>
    /// Initializes a provider using configured options (supports either ApiToken or OAuth2).
    /// </summary>
    public HipcallTokenProvider(
        IOptions<HipcallClientOptions> options,
        ITokenStore? tokenStore = null,
        HipcallOAuthClient? oauthClient = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _tokenStore = tokenStore;
        _oauthClient = oauthClient;
        AllowedHostsValidator = new AllowedHostsValidator(DefaultAllowedHosts);
    }

    /// <inheritdoc />
    public AllowedHostsValidator AllowedHostsValidator { get; }

    /// <inheritdoc />
    public async Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        // Never leak the token to hosts other than Hipcall.
        if (!AllowedHostsValidator.IsUrlHostValid(uri))
        {
            return string.Empty;
        }

        // 1. Static API Token mode (permanent, no expiry check needed)
        var tokenCandidate = _staticApiToken ?? _options?.Value?.ApiToken;
        if (!string.IsNullOrWhiteSpace(tokenCandidate))
        {
            return tokenCandidate;
        }

        // 2. OAuth2 flow mode
        if (_tokenStore is null || _oauthClient is null)
        {
            throw new InvalidOperationException(
                "Neither a permanent ApiToken nor OAuth2 credentials (ClientId/ClientSecret) were configured for Hipcall.");
        }

        var token = await GetStoredTokenOrThrowAsync().ConfigureAwait(false);
        if (!token.IsExpired())
        {
            return token.AccessToken;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Another caller may have refreshed the token while we waited for the lock.
            token = await GetStoredTokenOrThrowAsync().ConfigureAwait(false);
            if (!token.IsExpired())
            {
                return token.AccessToken;
            }

            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                throw new InvalidOperationException(
                    "The Hipcall access token has expired and there is no refresh token to renew it.");
            }

            var refreshed = await _oauthClient
                .RefreshTokenAsync(token.RefreshToken, cancellationToken)
                .ConfigureAwait(false);

            // Keep the old refresh token if the server does not rotate it.
            if (string.IsNullOrEmpty(refreshed.RefreshToken))
            {
                refreshed = refreshed with { RefreshToken = token.RefreshToken };
            }

            await _tokenStore.SaveTokenAsync(refreshed).ConfigureAwait(false);
            return refreshed.AccessToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<OAuthTokenResponse> GetStoredTokenOrThrowAsync() =>
        await _tokenStore!.GetTokenAsync().ConfigureAwait(false)
        ?? throw new InvalidOperationException(
            "No Hipcall token is available. Exchange an authorization code with " +
            "HipcallOAuthClient.ExchangeCodeAsync and save the result to the ITokenStore first, " +
            "or configure a permanent ApiToken.");
}
