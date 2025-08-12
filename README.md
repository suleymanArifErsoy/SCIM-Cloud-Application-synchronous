# SCIM Application

## 📌 Proje Açıklaması (Türkçe)
Bu proje, **SCIM (System for Cross-domain Identity Management)** protokolünü kullanarak farklı uygulamalar arasında kullanıcı yönetimini otomatikleştirmek amacıyla geliştirilmiş bir **ASP.NET Core MVC** uygulamasıdır.  
Admin paneli üzerinden kullanıcı oluşturma, güncelleme ve silme işlemleri yapılabilir. Bu işlemler SCIM destekli harici uygulamalara otomatik olarak senkronize edilir.

### ✨ Özellikler
- SCIM protokolü ile entegrasyon
- Kullanıcı ekleme, güncelleme, silme
- Farklı uygulamalar için SCIM endpoint ve kimlik bilgisi yönetimi
- Bearer Token ve API Key desteği
- MVC tabanlı web arayüzü
- SQL Server veya PostgreSQL desteği

### 🛠 Teknolojiler
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SCIM v2 Protokolü**
- **C#**
- **Bootstrap / HTML / CSS / JavaScript**

### 📂 Proje Yapısı
- **Models/** → Veri modelleri (ör: `Application`, `User`, `UserApplication`)
- **Services/** → SCIM servis arayüzleri ve implementasyonları (`IScimService`)
- **Controllers/** → MVC controller sınıfları
- **Views/** → Razor tabanlı kullanıcı arayüzü sayfaları

### 🚀 Kurulum
1. Projeyi klonlayın:
   ```bash
   git clone https://github.com/kullaniciadi/SCIMApplication.git
   cd SCIMApplication

