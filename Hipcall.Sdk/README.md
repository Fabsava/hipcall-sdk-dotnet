# Hipcall .NET SDK (Deneme)

[![NuGet Version](https://img.shields.io/nuget/v/Hipcall.Sdk.Deneme.svg?style=flat-square)](https://www.nuget.org/packages/Hipcall.Sdk.Deneme)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg?style=flat-square)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

Hipcall API V3 ile C# ve .NET projelerinizde tam entegrasyon sağlamak için geliştirilmiş, Kiota tabanlı resmi/deneme .NET SDK'sidir. Bu kütüphane, Hipcall'ın sunduğu telekomünikasyon, CRM ve iletişim altyapısını .NET ekosistemine modern, tip güvenli (type-safe) ve yüksek performanslı bir şekilde taşımayı hedefler.

## 🚀 Özellikler

* **Tam API Kapsamı:** Hipcall API V3 üzerindeki tüm endpointler ve veri modelleri desteklenir.
* **Modern Mimari:** Microsoft Kiota ile üretilmiş, standartlara uygun HTTP istemcisi.
* **Otomatik Kimlik Doğrulama:** OAuth2 `authorization_code` ve `refresh_token` akışlarını otomatik yöneten token altyapısı.
* **Dependency Injection (DI):** ASP.NET Core ve modern Worker Service projeleri için `IServiceCollection` entegrasyonu.
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

### 1. Servisleri Yapılandırma (Dependency Injection)

`Program.cs` veya `Startup.cs` dosyanızda Hipcall SDK'sini servislere ekleyin:

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

### 2. İstemciyi Kullanma

SDK servislere eklendikten sonra, herhangi bir sınıfınızda `HipcallClient` nesnesini Dependency Injection (Constructor) üzerinden çağırarak API'ye istek atabilirsiniz:

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
            // İstemci, arka planda token yönetimini otomatik yapar
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

## 🔒 Token Yönetimi

SDK, varsayılan olarak token'ları bellekte (RAM) tutan `InMemoryTokenStore` kullanır. Üretim (Production) ortamında token'ları veritabanınızda veya Redis'te saklamak için kendi `ITokenStore` uygulamanızı yazıp servislere ekleyebilirsiniz:

```csharp
using Hipcall.Sdk.Deneme.Authentication;

public class MyDatabaseTokenStore : ITokenStore
{
    // Kendi veritabanı kaydetme/okuma mantığınızı buraya yazın
    public Task SaveTokenAsync(OAuthTokenResponse token) => Task.CompletedTask;
    public Task<OAuthTokenResponse?> GetTokenAsync() => Task.FromResult<OAuthTokenResponse?>(null);
    public Task DeleteTokenAsync() => Task.CompletedTask;
}

// Program.cs
builder.Services.AddScoped<ITokenStore, MyDatabaseTokenStore>();
builder.Services.AddHipcallSdk(options => { ... });
```

## 📄 Lisans

Bu proje [MIT Lisansı](https://opensource.org/licenses/MIT) ile lisanslanmıştır.
