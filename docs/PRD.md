# PRD — Telefon Rehberi Uygulaması (Phone Book Application)

## 📋 Doküman Bilgileri

| Alan | Değer |
|------|-------|
| **Proje Adı** | Contacts ARGE24 |
| **Versiyon** | 1.1 |
| **Tarih** | 2026-02-09 |
| **Teknoloji Stack** | PostgreSQL (Docker) + .NET Core Web API + Angular |

---

## 1. Amaç ve Kapsam

Kullanıcının kişi kayıtlarını (isim/soyisim/telefon/e-posta vb.) oluşturup yönetebildiği, arama–filtreleme yapabildiği bir telefon rehberi web uygulaması.

### ✅ Kapsam İçi
- Kişi CRUD (Create/Read/Update/Delete)
- Listeleme, arama, filtreleme, sıralama
- Kişi detay ekranı
- Favoriler (yıldız) ve etiketleme (tags)
- İçeri/dışarı aktarım (CSV)
- **🌐 Çoklu Dil Desteği (Türkçe / English)**

### ❌ Kapsam Dışı (V2 için)
- Çoklu kullanıcı / yetkilendirme
- Mobil uygulama
- Çağrı/SMS entegrasyonu

---

## 2. Hedef Kullanıcı ve Kullanım Senaryoları

### Persona
Günlük rehberini web'den tutmak isteyen kullanıcı/ekip.

### Ana Senaryolar
1. Kullanıcı yeni kişi ekler (ad, soyad, telefon, e-posta)
2. Kişi listesinden hızlı arama yapar ("Ali", "+90 5xx")
3. Kişiyi favoriye alır ve "Favoriler" filtresinden görüntüler
4. Etiket ekler ("İş", "Aile", "Staj")
5. CSV'den toplu import yapar / mevcut rehberi export eder
6. **Uygulama dilini Türkçe/İngilizce arasında değiştirir**

---

## 3. MVP Fonksiyonel Gereksinimler

### 3.1 Kişi Yönetimi

#### FR-001: Kişi Oluşturma
| Alan | Tip | Zorunlu | Açıklama |
|------|-----|---------|----------|
| Ad (FirstName) | string | ✅ | Maksimum 100 karakter |
| Soyad (LastName) | string | ✅ | Maksimum 100 karakter |
| Telefon (Phone) | string | ✅ | Benzersiz, maksimum 30 karakter |
| E-posta (Email) | string | ❌ | Format validasyonu |
| Şirket (Company) | string | ❌ | Maksimum 200 karakter |
| Not (Notes) | string | ❌ | Sınırsız metin |
| Favori (IsFavorite) | boolean | ❌ | Varsayılan: false |
| Etiketler (Tags) | array | ❌ | 0..n etiket |

> Sistem, validasyon hatalarını kullanıcıya anlaşılır döndürür.

#### FR-002: Kişi Listeleme
- **Sayfalama:** Varsayılan 20 kayıt/sayfa
- **Sıralama:** Ad, Soyad, Oluşturma Tarihi
- **Filtreler:** Favoriler, Etiket, Şirket

#### FR-003: Kişi Detayı
- Kişinin tüm bilgilerini gösterir
- "Düzenle", "Sil", "Favori" aksiyonları

#### FR-004: Kişi Güncelleme
- Tüm alanlar düzenlenebilir
- Telefon/e-posta validasyonları uygulanır

#### FR-005: Kişi Silme
- **Soft Delete** yaklaşımı: `IsDeleted`, `DeletedAt`
- UI'da geri alma opsiyonu (opsiyonel)

### 3.2 Arama

#### FR-006: Global Arama
- Tek arama kutusu: ad/soyad/telefon/e-posta/şirket içinde arar
- "contains" arama yeterli

### 3.3 Etiketler

#### FR-007: Etiket Yönetimi
- Kişiye etiket ekle/çıkar
- Etiketler listeden filtrelemede kullanılabilir

### 3.4 CSV Import/Export

#### FR-008: CSV Export
- Filtrelenmiş liste CSV'ye aktarılabilir

#### FR-009: CSV Import
- CSV yükle, satır bazında hata raporu
- Başarılı olanlar kaydedilir, hatalı satırlar kullanıcıya gösterilir

### 3.5 Çoklu Dil Desteği

#### FR-010: Dil Seçimi
- Türkçe ve İngilizce dil desteği
- Dil tercihi tarayıcıda saklanır (localStorage)
- Tüm UI metinleri, hata mesajları ve validasyon mesajları çevrilir

---

## 4. Non-Functional (Kalite) Gereksinimler

| ID | Gereksinim | Açıklama |
|----|------------|----------|
| NFR-001 | Performans | 10.000 kişide listede sayfalama ile < 500ms |
| NFR-002 | Validasyon | Backend zorunlu; frontend UX için |
| NFR-003 | Loglama | API istekleri ve hata logları (console minimum) |
| NFR-004 | Güvenlik | Input sanitization, düzgün hata mesajı (stack trace dönme) |
| NFR-005 | Docker | PostgreSQL Docker'da çalışır |
| NFR-006 | Test | API için unit/integration test |
| NFR-007 | i18n | Angular i18n veya ngx-translate ile çoklu dil |

---

## 5. UX / Ekranlar

### 5.1 Kişiler Listesi
- Arama barı
- Filtreler: Favoriler, Etiket, Şirket
- Tablo/Kart görünümü
- "Yeni Kişi" butonu
- **Dil değiştirme butonu (🇹🇷/🇬🇧)**

### 5.2 Kişi Ekle / Düzenle
- Form + inline validasyon

### 5.3 Kişi Detay
- Bilgiler + Favori toggle + Etiketler

### 5.4 Import/Export
- CSV indir / CSV yükle + sonuç raporu

---

## 6. Veri Modeli

### Contact
```
Id              : GUID veya int (PK)
FirstName       : nvarchar(100) - NOT NULL
LastName        : nvarchar(100) - NOT NULL
Phone           : nvarchar(30)  - NOT NULL, INDEX
Email           : nvarchar(200) - NULL
Company         : nvarchar(200) - NULL
Notes           : nvarchar(max) - NULL
IsFavorite      : bit           - DEFAULT 0
CreatedAt       : datetime2     - NOT NULL
UpdatedAt       : datetime2     - NULL
IsDeleted       : bit           - DEFAULT 0
DeletedAt       : datetime2     - NULL
```

### Tag
```
Id              : int (PK)
Name            : nvarchar(50)  - UNIQUE, NOT NULL
```

### ContactTag (Many-to-Many)
```
ContactId       : FK -> Contact
TagId           : FK -> Tag
PRIMARY KEY (ContactId, TagId)
```

---

## 7. API Sözleşmesi

**Base URL:** `/api`

### Contacts
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/contacts` | Liste (query: search, page, pageSize, sort, favoriteOnly, tag) |
| GET | `/api/contacts/{id}` | Detay |
| POST | `/api/contacts` | Oluştur |
| PUT | `/api/contacts/{id}` | Güncelle |
| DELETE | `/api/contacts/{id}` | Sil (soft delete) |

### Tags
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/tags` | Tüm etiketler |
| POST | `/api/tags` | Etiket oluştur |
| DELETE | `/api/tags/{id}` | Etiket sil |

### CSV
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/contacts/export` | CSV export (query: search, tag) |
| POST | `/api/contacts/import` | CSV import (multipart/form-data) |

---

## 8. Kabul Kriterleri

| # | Kriter |
|---|--------|
| AC-01 | Yeni kişi eklediğimde listeye düşmeli ve detayına gidebilmeliyim |
| AC-02 | Telefon formatı boş olamaz; e-posta formatı geçersizse API 400 dönmeli |
| AC-03 | Arama kutusuna "555" yazınca telefonunda 555 geçenler gelmeli |
| AC-04 | Favoriye aldığım kişi "Favoriler" filtresinde görünmeli |
| AC-05 | CSV import'ta hatalı satırlar ayrı raporlanmalı; doğru satırlar kaydolmalı |
| AC-06 | Dil değiştirildiğinde tüm UI metinleri anında güncellenmelidir |

---

## 9. Proje Fazları

### Phase 1 — MVP (1 Hafta)
- [x] Proje setup (Docker + .NET + Angular)
- [ ] Contacts CRUD
- [ ] Liste/arama
- [ ] Favori özelliği
- [ ] Basic tags
- [ ] Çoklu dil desteği (TR/EN)

### Phase 2 — Gelişmiş Özellikler
- [ ] CSV import/export
- [ ] Soft delete + geri alma
- [ ] Audit (created/updated logs)

### Phase 3 — V2 (Opsiyonel)
- [ ] Login + kullanıcı bazlı rehber
- [ ] Paylaşılan rehber / ekip rehberi

---

## 10. Ek "Güzel Duran" Özellikler (Opsiyonel)

| Özellik | Açıklama |
|---------|----------|
| 🔄 Duplikasyon Uyarısı | Aynı telefon numarasıyla eklerken uyar |
| ⌨️ Hızlı Arama Kısayolu | `/` tuşu arama kutusunu odaklar |
| 📅 Son Güncellenenler | Son eklenen/güncellenen kişiler filtresi |
| 📇 VCard Export | VCF formatında export (MVP dışı) |

---

## 11. Teknik Mimari

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend                              │
│                    Angular (Port 4200)                       │
│              • ngx-translate (i18n)                          │
│              • Angular Material UI                           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                        Backend                               │
│                .NET Core Web API (Port 5050)                 │
│              • Entity Framework Core                         │
│              • AutoMapper                                    │
│              • FluentValidation                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Database                               │
│              PostgreSQL 16 (Docker - Port 5432)              │
└─────────────────────────────────────────────────────────────┘
```

---

## 12. Git Yapılandırması

Proje Git versiyon kontrolü altında tutulacaktır.

### Branch Stratejisi
- `main` - Production-ready kod
- `develop` - Geliştirme branchi
- `feature/*` - Özellik geliştirme

### Commit Mesaj Formatı
```
feat: yeni özellik eklendi
fix: hata düzeltildi
docs: dokümantasyon güncellendi
refactor: kod iyileştirmesi
test: test eklendi/güncellendi
```

---

## 13. Geliştirme Dili Kuralı

- Proje codebase'i (dosya/klasör adları, class/function/variable isimleri, API endpointleri, DTO alanları, commit başlıkları) İngilizce terimler kullanılarak geliştirilmelidir.
- Kod içi yorumlar (comments) Türkçe olabilir.
