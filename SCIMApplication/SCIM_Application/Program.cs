using Microsoft.EntityFrameworkCore;
using SCIM_Application.Data;
using SCIM_Application.Services;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddHttpClient();
builder.Services.AddScoped<IScimService, ScimService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Geliştirme için veritabanını oluştur ve başlangıç veri düzenlemeleri
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    db.Database.EnsureCreated();

    // Okta uygulamalarını kaldır/devre dışı bırak
    var oktaApps = db.Applications.Where(a => a.Provider.ToLower() == "okta").ToList();
    if (oktaApps.Any())
    {
        db.Applications.RemoveRange(oktaApps);
        db.SaveChanges();
    }

    

    // Auth0 access token'dan domain'i çıkar ve Application oluştur/güncelle
    var auth0Token = config["Auth0:AccessToken"];
    var tokenType = config["Auth0:TokenType"] ?? "Bearer";
    if (!string.IsNullOrWhiteSpace(auth0Token))
    {
        string? issuer = TryGetIssuerFromJwt(auth0Token);
        // Örn: https://your-tenant.us.auth0.com/
        var baseUrl = issuer?.TrimEnd('/');
        // SCIM 2.0 standart endpoint formatı: /api/v2/scim/v2/Users
        var scimEndpoint = string.IsNullOrWhiteSpace(baseUrl)
            ? string.Empty
            : $"{baseUrl}/api/v2/scim/v2/Users";

        var auth0App = db.Applications.FirstOrDefault(a => a.Provider.ToLower() == "auth0");
        if (auth0App == null)
        {
            db.Applications.Add(new SCIM_Application.Models.Application
            {
                Name = "Auth0",
                Description = "Auth0 SCIM 2.0 Entegrasyonu",
                Provider = "Auth0",
                ScimEndpoint = scimEndpoint,
                ApiKey = string.Empty,
                BearerToken = tokenType.Equals("Bearer", StringComparison.OrdinalIgnoreCase) ? auth0Token : auth0Token,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
        else
        {
            auth0App.BearerToken = tokenType.Equals("Bearer", StringComparison.OrdinalIgnoreCase) ? auth0Token : auth0Token;
            if (!string.IsNullOrWhiteSpace(scimEndpoint))
                auth0App.ScimEndpoint = scimEndpoint;
            auth0App.UpdatedAt = DateTime.UtcNow;
            db.SaveChanges();
        }
    }
}

app.Run();

static string? TryGetIssuerFromJwt(string jwt)
{
    try
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return null;
        static byte[] FromBase64Url(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
        var payloadJson = Encoding.UTF8.GetString(FromBase64Url(parts[1]));
        using var doc = JsonDocument.Parse(payloadJson);
        if (doc.RootElement.TryGetProperty("iss", out var issElem))
        {
            return issElem.GetString();
        }
        return null;
    }
    catch
    {
        return null;
    }
}
