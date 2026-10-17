using Hipcall.Sdk.Deneme.Authentication;
using Hipcall.Sdk.Deneme.Client;
using Hipcall.Sdk.Deneme.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("=== Hipcall SDK Başlatma Örnekleri ===");

// ---------------------------------------------------------------------------------------------
// YÖNTEM 1: Sabit / Kalıcı API Token ile Kullanım (Dependency Injection ile)
// Hipcall Web Panel: Ayarlar -> Geliştirici -> API Tokenları kısmından aldığınız kalıcı token.
// Sayfa yenilense de, servis yeniden başlasa da tekrar giriş yapmanız gerekmez.
// ---------------------------------------------------------------------------------------------
var services = new ServiceCollection();

// İster doğrudan token vererek:
services.AddHipcallSdk("demo_api_token_here");

// İsterseniz options üzerinden:
// services.AddHipcallSdk(options =>
// {
//     options.ApiToken = "demo_api_token_here";
// });

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var diClient = scope.ServiceProvider.GetRequiredService<HipcallClient>();
Console.WriteLine("[1] DI üzerinden kalıcı API Token ile HipcallClient başarıyla başlatıldı!");


// ---------------------------------------------------------------------------------------------
// YÖNTEM 2: DI Olmadan Tek Satırda API Token ile İstemci Oluşturma
// Konsol uygulamaları, arka plan işleri veya betikler için en hızlı yöntem.
// ---------------------------------------------------------------------------------------------
var directClient = HipcallClient.Create("demo_api_token_here");
Console.WriteLine("[2] DI olmadan doğrudan HipcallClient.Create ile istemci başarıyla oluşturuldu!");


// ---------------------------------------------------------------------------------------------
// YÖNTEM 3: OAuth2 Authorization Code Akışı (Kullanıcı Bazlı Yetkilendirme)
// Eğer uygulamanız çok kullanıcılı bir SaaS ise ve her kullanıcı kendi Hipcall hesabıyla
// giriş yapacaksa bu akış kullanılır:
//
// services.AddHipcallSdk(options =>
// {
//     options.ClientId = "your-client-id";
//     options.ClientSecret = "your-client-secret";
//     options.RedirectUri = "https://app.example.com/callback";
// });
// ---------------------------------------------------------------------------------------------

Console.WriteLine("Tüm yöntemler hazır ve başarıyla çalıştı!");
