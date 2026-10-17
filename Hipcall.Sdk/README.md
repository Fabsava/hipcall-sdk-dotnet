# Hipcall .NET SDK (Deneme)

[![NuGet Version](https://img.shields.io/nuget/v/Hipcall.Sdk.Deneme.svg?style=flat-square)](https://www.nuget.org/packages/Hipcall.Sdk.Deneme)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg?style=flat-square)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

Hipcall API V3 ile C# ve .NET projelerinizde tam entegrasyon sağlamak için geliştirilmiş, Kiota tabanlı resmi/deneme .NET SDK'sidir. Bu kütüphane, Hipcall'ın sunduğu telekomünikasyon, CRM ve iletişim altyapısını .NET ekosistemine modern, tip güvenli (type-safe) ve yüksek performanslı bir şekilde taşımayı hedefler.

## 🚀 Özellikler

* **Tam API Kapsamı:** Hipcall API V3 üzerindeki tüm endpointler ve veri modelleri desteklenir.
* **Kalıcı API Token Desteği:** Hipcall web panelinden alacağınız sabit API token ile sayfa yenilense de oturum kaybetmeden kalıcı bağlantı.
* **OAuth2 Flow Desteği:** Çok kullanıcılı uygulamalar için `authorization_code` ve `refresh_token` akışlarını otomatik yöneten token altyapısı.
* **Modern Mimari:** Microsoft Kiota ile üretilmiş, standartlara uygun HTTP istemcisi.
* **Dependency Injection (DI):** ASP.NET Core ve modern Worker Service projeleri için `IServiceCollection` entegrasyonu.
* **Hızlı Kurulum (Non-DI):** Tek satırda `HipcallClient.Create("token")` ile istemci oluşturma imkanı.
* **Genişletilebilir Token Saklama:** `ITokenStore` arayüzü ile token'ları bellek, veritabanı veya dosyada saklama özgürlüğü.

## 📦 Kurulum

Paketi projenize NuGet üzerinden dahil edebilirsiniz:

**.NET CLI ile:**
```bash
dotnet add package Hipcall.Sdk.Deneme
```

**Package Manager Console ile:**
```powershell
Install-Package Hipcall.Sdk.Deneme
```

## 🛠️ Hızlı Başlangıç (Quick Start)

SDK iki farklı kimlik doğrulama yöntemini destekler:
1. **API Token (Önerilen - Kalıcı & Basit):** Hipcall web panelinden (`Ayarlar -> Geliştirici -> API Tokenları`) üretilen kalıcı token. Sayfa yenilense veya sunucu yeniden başlasa bile tekrar giriş yapmanız gerekmez.
2. **OAuth2 Authorization Code:** Çok kullanıcılı SaaS uygulamalarında her kullanıcının kendi Hipcall hesabıyla yetkilendirilmesi gereken senaryolar için.

---

### Yöntem 1: Kalıcı API Token ile Kullanım (En Pratik)

#### A) Dependency Injection ile (ASP.NET Core / Worker Service)
`Program.cs` dosyanızda tek satırla ekleyin:

```csharp
using Hipcall.Sdk.Deneme.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Doğrudan token vererek veya Configuration üzerinden:
builder.Services.AddHipcallSdk(builder.Configuration["Hipcall:ApiToken"]!);

// veya options üzerinden:
// builder.Services.AddHipcallSdk(options => {
//     options.ApiToken = builder.Configuration["Hipcall:ApiToken"];
// });

var app = builder.Build();
```

#### B) DI Olmadan (Konsol / Script / Arka Plan Görevleri)
```csharp
using Hipcall.Sdk.Deneme.Client;

// Tek satırda istemciyi oluşturun
var client = HipcallClient.Create("your_api_token_here");

var profile = await client.Api.V3.Profile.GetAsync();
Console.WriteLine($"Kullanıcı: {profile?.Data?.User?.FullName}");
```

---

### Yöntem 2: OAuth2 Akışı ile Kullanım

`Program.cs` dosyanızda OAuth2 ayarlarınızı girin:

```csharp
using Hipcall.Sdk.Deneme.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHipcallSdk(options =>
{
    options.ClientId = builder.Configuration["Hipcall:ClientId"];
    options.ClientSecret = builder.Configuration["Hipcall:ClientSecret"];
    options.RedirectUri = builder.Configuration["Hipcall:RedirectUri"];
});

var app = builder.Build();
```

OAuth2 kullanıldığında, varsayılan olarak token'lar bellekte (`InMemoryTokenStore`) tutulur. Sayfa yenilemelerinde veya uygulama yeniden başladığında oturumun kaybolmaması için kendi `ITokenStore` uygulamanızı (EF Core, Redis vb.) servislere ekleyebilirsiniz:

```csharp
using Hipcall.Sdk.Deneme.Authentication;

public class MyDatabaseTokenStore : ITokenStore
{
    public Task SaveTokenAsync(OAuthTokenResponse token) { /* DB'ye kaydet */ return Task.CompletedTask; }
    public Task<OAuthTokenResponse?> GetTokenAsync() { /* DB'den oku */ return Task.FromResult<OAuthTokenResponse?>(null); }
    public Task DeleteTokenAsync() { /* DB'den sil */ return Task.CompletedTask; }
}

// Program.cs
builder.Services.AddScoped<ITokenStore, MyDatabaseTokenStore>();
builder.Services.AddHipcallSdk(options => { ... });
```

---

### İstemciyi Controller / Servis İçinde Kullanma

İstemciyi Dependency Injection üzerinden çekip doğrudan API çağrısı yapabilirsiniz:

```csharp
using Hipcall.Sdk.Deneme.Client;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CallController : ControllerBase
{
    private readonly HipcallClient _hipcallClient;

    public CallController(HipcallClient hipcallClient)
    {
        _hipcallClient = hipcallClient;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        try
        {
            var profile = await _hipcallClient.Api.V3.Profile.GetAsync();
            return Ok(profile);
        }
        catch (Exception ex)
        {
            return BadRequest($"Hipcall API Hatası: {ex.Message}");
        }
    }
}
```

## 📄 Lisans

Bu proje [MIT Lisansı](https://opensource.org/licenses/MIT) ile lisanslanmıştır.
