using System.Text.Json.Serialization;

namespace Hipcall.Sdk.Authentication;

/// <summary>
/// Successful response of the Hipcall <c>POST /oauth/token</c> endpoint.
/// </summary>
public sealed record OAuthTokenResponse
{
    /// <summary>The bearer access token.</summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>The refresh token, used to obtain a new access token. May be absent.</summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>Access token lifetime in seconds, counted from <see cref="ObtainedAt"/>.</summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>The token type, normally <c>Bearer</c>.</summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// When the token was received. The server does not send this; it defaults to the moment the
    /// object is created (i.e. deserialized), and is serialized so persisted tokens keep it.
    /// </summary>
    [JsonPropertyName("obtained_at")]
    public DateTimeOffset ObtainedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>The instant the access token stops being valid.</summary>
    [JsonIgnore]
    public DateTimeOffset ExpiresAt => ObtainedAt.AddSeconds(ExpiresIn);

    /// <summary>
    /// Returns true when the access token is missing or expires within <paramref name="leeway"/>
    /// (default 60 seconds, so a token is refreshed slightly before it really expires).
    /// </summary>
    public bool IsExpired(TimeSpan? leeway = null) =>
        string.IsNullOrEmpty(AccessToken)
        || DateTimeOffset.UtcNow >= ExpiresAt - (leeway ?? TimeSpan.FromSeconds(60));
}
