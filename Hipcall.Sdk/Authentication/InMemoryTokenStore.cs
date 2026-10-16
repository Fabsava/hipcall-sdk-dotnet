namespace Hipcall.Sdk.Deneme.Authentication;

/// <summary>
/// Keeps the token in process memory only. Meant for first tests; the token is lost on restart.
/// </summary>
public class InMemoryTokenStore : ITokenStore
{
    private volatile OAuthTokenResponse? _token;

    public Task SaveTokenAsync(OAuthTokenResponse token)
    {
        ArgumentNullException.ThrowIfNull(token);
        _token = token;
        return Task.CompletedTask;
    }

    public Task<OAuthTokenResponse?> GetTokenAsync() => Task.FromResult(_token);

    public Task DeleteTokenAsync()
    {
        _token = null;
        return Task.CompletedTask;
    }
}
