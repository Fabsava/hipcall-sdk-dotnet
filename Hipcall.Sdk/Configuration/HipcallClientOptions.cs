namespace Hipcall.Sdk.Deneme.Configuration;

/// <summary>
/// Options for the Hipcall SDK. Bind or configure them via
/// <c>services.AddHipcallSdk(o =&gt; { ... })</c>.
/// </summary>
public class HipcallClientOptions
{
    /// <summary>OAuth2 client id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>OAuth2 client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Redirect URI registered for the OAuth2 application.</summary>
    public string RedirectUri { get; set; } = string.Empty;
}
