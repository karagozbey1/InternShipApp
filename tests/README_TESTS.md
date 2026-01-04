# InternshipTrackingSystem Test Projesi

## Test Stratejisi

Bu proje iki tip test içerir:

### 1. Unit Tests (InternshipTrackingSystem.UnitTests)
- **Amaç:** Business logic ve utility sýnýflarýný izole ortamda test etmek
- **Araçlar:** xUnit, Moq, FluentAssertions
- **Kapsam:**
  - PasswordSecurity (Hash/Verify)
  - UserService (CRUD, validation)
  - InternshipApprovalService (form iþlemleri)
  - InternshipOfferService (ilan CRUD)
  - UnitOfWork pattern (repository eriþimi, SaveAsync)

### 2. Integration Tests (InternshipTrackingSystem.IntegrationTests)
- **Amaç:** HTTP endpoint'lerini ve authorization kurallarýný gerçek ortama yakýn test etmek
- **Araçlar:** xUnit, Microsoft.AspNetCore.Mvc.Testing, FluentAssertions
- **Kapsam:**
  - HomeController (public sayfalar)
  - AccountController (login, register)
  - ApplicationController (student endpoints, role-based access)
- **Veritabaný:** EF Core InMemory (PostgreSQL yerine)
- **Authentication:** Custom TestAuthHandler ile role simülasyonu

---

## Proje Yapýsý

```
tests/
??? README_TESTS.md
??? InternshipTrackingSystem.UnitTests/
?   ??? Security/
?   ?   ??? PasswordSecurityTests.cs      (10 test)
?   ??? Services/
?   ?   ??? UserServiceTests.cs           (8 test)
?   ?   ??? InternshipApprovalServiceTests.cs (8 test)
?   ?   ??? InternshipOfferServiceTests.cs    (7 test)
?   ??? UnitOfWork/
?       ??? UnitOfWorkTests.cs            (6 test)
?
??? InternshipTrackingSystem.IntegrationTests/
    ??? Infrastructure/
    ?   ??? CustomWebApplicationFactory.cs
    ?   ??? TestAuthHandler.cs
    ??? Controllers/
        ??? HomeControllerTests.cs        (9 test)
        ??? AccountControllerTests.cs     (8 test)
        ??? ApplicationControllerTests.cs (9 test)
```

**Toplam:** ~65 test

---

## Terminal Komutlarý

### Tüm Testleri Çalýþtýr
```bash
dotnet test
```

### Sadece Unit Testleri Çalýþtýr
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Sadece Integration Testleri Çalýþtýr
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### Code Coverage ile Çalýþtýr
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### HTML Coverage Raporu Oluþtur
```bash
# Önce testleri coverage ile çalýþtýr
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# Sonra HTML rapor oluþtur
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/coverage-report" -reporttypes:Html
```

### Belirli Bir Test Sýnýfýný Çalýþtýr
```bash
dotnet test --filter "FullyQualifiedName~PasswordSecurityTests"
```

### Verbose Çýktý ile Çalýþtýr
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## Coverage Raporu

Coverage raporu oluþturulduktan sonra:
- **Konum:** `TestResults/coverage-report/index.html`
- Tarayýcýda açarak satýr bazýnda coverage görebilirsiniz

### ReportGenerator Kurulumu (Bir kez yapýlýr)
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

---

## Slayt Kanýtý Ýçin Ekran Görüntüleri

### 1. Test Sonuçlarý
Terminal çýktýsý:
```
Passed!  - Failed: 0, Passed: 65, Skipped: 0, Total: 65
```

### 2. Coverage Raporu
`TestResults/coverage-report/index.html` dosyasýný tarayýcýda açýn:
- Line Coverage: %XX
- Branch Coverage: %XX
- Her proje için detaylý breakdown

---

## Test Senaryolarý Özeti

### Unit Tests

| Test Sýnýfý | Test Sayýsý | Açýklama |
|-------------|-------------|----------|
| PasswordSecurityTests | 10 | Hash/Verify, edge cases, unicode |
| UserServiceTests | 8 | CRUD, duplicate email, password change |
| InternshipApprovalServiceTests | 8 | Form CRUD, validation |
| InternshipOfferServiceTests | 7 | Offer CRUD, active filter |
| UnitOfWorkTests | 6 | Repository access, SaveAsync |

### Integration Tests

| Test Sýnýfý | Test Sayýsý | Açýklama |
|-------------|-------------|----------|
| HomeControllerTests | 9 | Public pages, API endpoints |
| AccountControllerTests | 8 | Login, Register, Auth redirect |
| ApplicationControllerTests | 9 | Student pages, role-based access |

---

## Notlar

- **InMemory Database:** Integration testlerde PostgreSQL yerine EF Core InMemory kullanýlýr
- **Test Isolation:** Her test kendi database instance'ýný kullanýr (GUID ile)
- **Seed Data:** Test database'ine otomatik örnek veri eklenir
- **Authentication:** `X-Test-UserId`, `X-Test-UserRole` header'larý ile simüle edilir

---

## Hýzlý Baþlangýç

```bash
# 1. Solution root dizinine git
cd "C:\Users\Yusuf Karagöz\Documents\Visual Studio 2022\Repos\InternshipTrackingSystem"

# 2. Tüm testleri çalýþtýr
dotnet test

# 3. Coverage raporu oluþtur
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/coverage-report" -reporttypes:Html

# 4. Raporu aç
start TestResults/coverage-report/index.html
```
