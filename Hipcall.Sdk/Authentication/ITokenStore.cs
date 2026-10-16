namespace Hipcall.Sdk.Deneme.Authentication;

/// <summary>
/// Persists the OAuth token between requests (and, in your implementation, between restarts).
/// </summary>
public interface ITokenStore
{
    Task SaveTokenAsync(OAuthTokenResponse token);

    Task<OAuthTokenResponse?> GetTokenAsync();

    Task DeleteTokenAsync();
}
