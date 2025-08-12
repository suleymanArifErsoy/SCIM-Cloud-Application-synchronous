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
📌 Project Description (English)
This project is an ASP.NET Core MVC application developed to automate user management across multiple applications using the SCIM (System for Cross-domain Identity Management) protocol.
Through the admin panel, users can be created, updated, and deleted. These changes are automatically synchronized with external SCIM-enabled applications.

✨ Features
Integration with SCIM protocol

User creation, update, and deletion

SCIM endpoint & credentials management for multiple applications

Support for Bearer Token and API Key authentication

MVC-based web interface

SQL Server or PostgreSQL support

🛠 Technologies
ASP.NET Core MVC

Entity Framework Core

SCIM v2 Protocol

C#

Bootstrap / HTML / CSS / JavaScript

📂 Project Structure
Models/ → Data models (e.g., Application, User, UserApplication)

Services/ → SCIM service interfaces and implementations (IScimService)

Controllers/ → MVC controllers

Views/ → Razor-based UI pages

🚀 Installation
Clone the project:

bash
Copy
Edit
git clone https://github.com/username/SCIMApplication.git
cd SCIMApplication
