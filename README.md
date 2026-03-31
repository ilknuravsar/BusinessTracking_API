# Business Tracking System

Elektrik dağıtım şirketinin Arıza Takip Sistemi (Outage Management System) için geliştirilmiş bir RESTful API servisidir. Proje,  **.NET 8** üzerinde inşa edilmiştir.

## Teknik Özellikler ve Kullanılan Teknolojiler

- **Backend Çerçevesi:** ASP.NET Core Web API (.NET 8)
- **Veritabanı:** MSSQL Server (Entity Framework Core)
- **Kimlik Doğrulama & Yetkilendirme:** ASP.NET Core Identity & JWT (JSON Web Tokens)
- **Mimari:** Onion Architecture - Projenin bağımlılıklarını en aza indirmek, sistemi teknolojik değişimlere karşı esnek tutmak ve katmanlar arası izolasyon sayesinde yüksek test edilebilirliği sağlamak amacıyla bu mimari tercih edilmiştir.
- **Loglama:** Gömülü ILogger altyapısı ile konsol üzerinden izleme sağlanmıştır.
- **Hız Sınırlandırma (Rate Limiting):** API'nin güvenliğini sağlamak ve sistem kaynaklarını aşırı yüklenmeye karşı korumak amacıyla, kullanıcı başına dakikada maksimum 10 istek sınırı getiren Fixed Window hız sınırlandırma mekanizması uygulanmıştır.
- **API Dokümantasyonu:** Swagger / OpenAPI entegrasyonu (XML Yorumları etkin)
- **Birim Testleri (Unit Testing):** xUnit & Moq ile yazılmış Unit Test projeleri
- **Konteynerleştirme:** Docker & Docker Compose: Uygulamanın ve MSSQL veritabanının her ortamda (Local, Test, Prod) aynı standartlarda çalışabilmesi için Dockerize edilmiş; tüm servislerin tek bir komutla (docker-compose up) ayağa kaldırılabilmesi sağlanmıştır.

## Kullanılan Kütüphaneler

**Veritabanı ve ORM (Entity Framework Core)**

   - Microsoft.EntityFrameworkCore

   - Microsoft.EntityFrameworkCore.SqlServer

   - Microsoft.EntityFrameworkCore.Tools

   - Microsoft.AspNetCore.Identity.EntityFrameworkCore

   - Microsoft.EntityFrameworkCore.Design

   - Microsoft.AspNetCore.Identity.EntityFrameworkCore

**Kimlik Doğrulama ve Güvenlik (Identity & JWT)**

   - Microsoft.AspNetCore.Authentication.JwtBearer

   - Microsoft.AspNetCore.Identity

   - Microsoft.Extensions.Identity.Stores

**Eşleme ve Dokümantasyon (AutoMapper & Swagger)**

   - AutoMapper.Extensions.Microsoft.DependencyInjection

   - Swashbuckle.AspNetCore

   - AutoMapper

**Test (xUnit & Moq)**

   - xunit.runner.visualstudio

   - Moq

   - xunit


##  Arıza Kaydı Durum Yönetimi (State Machine)

**New(Yeni)** -> UnderReview(İnceleniyor), Cancelled(İptal Edildi)

**UnderReview(İnceleniyor)** -> Assigned(Atandı), Unfounded(Asılsız), Cancelled(İptal Edildi)

**Assigned(Atandı)** -> InProgress(Devam Ediyor), Cancelled(İptal Edildi)

**InProgress(Devam Ediyor)** -> Completed(Tamamlandı), Cancelled(İptal Edildi)

**Completed(Tamamlandı)** -> (Son Durum - Değiştirilemez)

**Cancelled(İptal Edildi)** -> (Son Durum - Değiştirilemez)

**Unfounded(Asılsız)** -> (Son Durum - Değiştirilemez) 

## Endpoint'ler ve Kullanımları

### Authentication (`/api/Auth/login`)
Sisteme erişebilmek için bir JWT Token üretilir. Seed verisinden Admin veya Standart kullanıcı girişleri yapılabilir.

### Arıza (Outage) Raporu Yönetimi

Bu bölümdeki tüm endpoint'ler [Authorize] ile korunmaktadır. Geçerli bir JWT Bearer Token girilmediği takdirde sistem 401 Unauthorized yanıtı dönecektir.

- **POST `/api/Outage/Create`:** Yeni bir arıza bildirimi açar. (Aynı lokasyona son 1 saatte yeni kayıt açılamaz).
- **GET `/api/Outage/GetAll`:** Arızaları listeleyerek sayfalama veya filtreleme (Durum, Öncelik vb.) imkanı yaratır. Admin tüm arızaları, kullanıcılar kendi açtığı arızaları görür.
- **GET `/api/Outage/GetById/{id}`:** Benzersiz bir ID ile arıza detayı getirir.
- **PUT `/api/Outage/{id}`:** Mevcut bir arıza bildiriminin içeriğini(açıklama, başlık, öncelik, konum) günceller. Kullanıcılar yalnızca kendilerine ait olan arızaları, adminler ise herkesin arızalarını değiştirebilir.
- **PATCH `/api/Outage/{id}/status`** (Sadece `Admin`): Bir arızanın mevcut durumunu uygun akış kuralları (State Machine) içinde değiştirir (ör: `New` -> `UnderReview`).

GetAll Metodunda **orderBy parametresi** boş bırakıldığında raporlar en yeni kayıttan itibaren azalan sırada, string olarak **priority** değeri gönderildiğinde ise Yüksek -> Orta -> Düşük (3-2-1) öncelik hiyerarşisine göre sıralanır.

## Docker ile Kurulum ve Çalıştırma

Projenin içinde bir `Dockerfile` ve bir **`docker-compose.yml`** oluşturulmuştur. Makinenize hiçbir .NET SDK veya MSSQL Server kurmadan projeyi hızlıca ayağa kaldırabilirsiniz.

### Ön Gereksinimler
- Makinenizde **Docker Desktop** (veya Docker CLI) yüklü ve çalışır durumda olmalıdır.

### Çalıştırma Adımları
1. Terminal veya komut satırını açıp proje ana dizinine (`docker-compose.yml` dosyasının bulunduğu dizin) gidin.
2. Sistemi Ayağa Kaldırın: Aşağıdaki komut ile veritabanı ve API servislerini otomatik olarak derleyip başlatın:
```bash
docker-compose up -d --build
```
3. Docker, önce MSSQL veritabanı konteynerini indirecek/başlatacak, sonrasında da projenin API `.NET 8` konteyner imajını derleyip yayına alacaktır.

## Docker ile Swagger Dokümantasyonu: 
**http://localhost:8080/swagger/index.html** üzerinden incelenebilir. 

## Veritabanı Yönetimi (Docker İçindeki SQL Server)
Docker konteyneri içinde çalışan SQL Server'a harici bir araçla (SSMS) bağlanmak için şu bilgileri kullanın:

- Server Name: localhost,1433

- Authentication: SQL Server Authentication

- Login: sa

- Password: P@ssw0rd2024!

## Yerel Geliştirme Ortamı Kurulum ve Çalıştırma

Bu yöntem için makinenizde .NET 8 SDK ve MSSQL Server kurulu olmalıdır.

1. Veritabanı Yapılandırması: API/appsettings.json dosyasındaki - ConnectionStrings bölümünü kendi yerel SQL Server adresinize göre güncelleyin.
("DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OutageDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true")

2. Veritabanı Migrasyonları: Proje kök dizininde şu komutları çalıştırarak veritabanı şemasını oluşturun:

**Yöntem A: Visual Studio (Package Manager Console)**
Eğer Visual Studio kullanıyorsanız, şu komutu çalıştırmanız yeterlidir:
Default Project olarak Infrastructure katmanını seçin.
Konsola şu komutu yazın: Update-Database

**Yöntem B: .NET CLI (Terminal)**
VS Code veya terminal kullanıyorsanız, proje kök dizininde şu komutu çalıştırın:
dotnet ef database update --project Infrastructure --startup-project API

3. Projeyi Başlatma: API klasörüne gidip projeyi normal .NET uygulaması olarak çalıştırın:

dotnet restore

dotnet run --project API

## Yerel Swagger Dokümantasyonu: 
**https://localhost:7295/swagger/index.html**

## İlk Giriş Bilgileri (Seed Data)
Uygulama ilk kez ayağa kalktığında DbSeeder mekanizması tarafından aşağıdaki test kullanıcıları otomatik olarak oluşturulur:

Admin Kullanıcı:

Username: ilknuradmin

Şifre: Admin123!

Standart Kullanıcı:

Username: ilknuruser

Şifre: User123!


## Birim Testlerini (Unit Tests) Çalıştırma

Birim testleri `Application.UnitTests` isimli ayrı bir kütüphane projesi vasıtasıyla yürütülmektedir.
Testlerin başarı durumunu CLI üzerinden test etmek için projenin kök dizininde şu komütü çalıştırın:
```bash
dotnet test
```
- Bu komut servislerin (OutageReportService) davranışlarını ve iş kuralı kararlarını güvence altına alır.
---

