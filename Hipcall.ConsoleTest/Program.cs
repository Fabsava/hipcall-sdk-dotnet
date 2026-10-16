using Hipcall.Sdk.Authentication;
using Hipcall.Sdk.Client;
using Hipcall.Sdk.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// 1) Create the service collection and register the Hipcall SDK with dummy settings.
var services = new ServiceCollection();

services.AddHipcallSdk(options =>
{
    options.ClientId = "dummy-client-id";
    options.ClientSecret = "dummy-client-secret";
    options.RedirectUri = "https://localhost:5001/callback";
});

// 2) Build the container.
using var serviceProvider = services.BuildServiceProvider();

// HipcallClient is registered as Scoped, so resolve it from a scope.
using var scope = serviceProvider.CreateScope();

// 3) Resolve HipcallClient from DI.
var client = scope.ServiceProvider.GetRequiredService<HipcallClient>();

Console.WriteLine("Hipcall SDK başarıyla başlatıldı ve DI konteynerinden çekildi!");

// ---------------------------------------------------------------------------------------------
// Example GET request (kept commented out: it needs real credentials and a stored token).
//
// Real flow:
//   1. Send the user to Hipcall's /oauth/authorize URL and receive the `code` on your RedirectUri.
//   2. Exchange the code for a token and save it in the ITokenStore:
//
//      var oauth = scope.ServiceProvider.GetRequiredService<HipcallOAuthClient>();
//      var tokenStore = scope.ServiceProvider.GetRequiredService<ITokenStore>();
//      var token = await oauth.ExchangeCodeAsync("<authorization-code>");
//      await tokenStore.SaveTokenAsync(token);
//
//   3. Call the API. The bearer token is added (and refreshed when expired) automatically:
//
//      var profile = await client.Api.V3.Profile.GetAsync();
//      Console.WriteLine($"Merhaba, {profile?.Data?.User?.FullName} ({profile?.Data?.User?.Email})");
// ---------------------------------------------------------------------------------------------
