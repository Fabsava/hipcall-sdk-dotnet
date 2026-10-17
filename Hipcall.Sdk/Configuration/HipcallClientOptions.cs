namespace Hipcall.Sdk.Deneme.Configuration;

/// <summary>
/// Options for the Hipcall SDK. Bind or configure them via
/// <c>services.AddHipcallSdk(o =&gt; { ... })</c>.
/// </summary>
public class HipcallClientOptions
{
    /// <summary>
    /// Static Hipcall API Token (created in web app under Settings -> Developer -> API tokens).
    /// If provided, this permanent token is used for all API requests and OAuth2 flow is bypassed.
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>OAuth2 client id.</summary>
    public string? ClientId { get; set; }

    /// <summary>OAuth2 client secret.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Redirect URI registered for the OAuth2 application.</summary>
    public string? RedirectUri { get; set; }
}
