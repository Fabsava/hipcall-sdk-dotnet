using Microsoft.Kiota.Abstractions.Authentication;

namespace Hipcall.Sdk.Deneme.Authentication;

/// <summary>
/// Kiota <see cref="IAccessTokenProvider"/> that reads the token from an <see cref="ITokenStore"/>
/// and refreshes it through <see cref="HipcallOAuthClient"/> once it has expired.
/// </summary>
public class HipcallTokenProvider : IAccessTokenProvider
{
    private static readonly string[] DefaultAllowedHosts = ["use.hipcall.com", "use.hipcall.com.tr"];

    private readonly ITokenStore _tokenStore;
    private readonly HipcallOAuthClient _oauthClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public HipcallTokenProvider(ITokenStore tokenStore, HipcallOAuthClient oauthClient)
    {
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _oauthClient = oauthClient ?? throw new ArgumentNullException(nameof(oauthClient));
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
        await _tokenStore.GetTokenAsync().ConfigureAwait(false)
        ?? throw new InvalidOperationException(
            "No Hipcall token is available. Exchange an authorization code with " +
            "HipcallOAuthClient.ExchangeCodeAsync and save the result to the ITokenStore first.");
}
