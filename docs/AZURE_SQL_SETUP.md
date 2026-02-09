# Azure SQL Veritabanı Kurulum Rehberi

Ücretsiz Azure SQL veritabanınızı oluşturmak için aşağıdaki adımları takip edin.

## 1. Veritabanı Oluşturma (Create SQL Database)

Ekran görüntüsündeki formu şu şekilde doldurun:

1.  **Subscription (Abonelik):** `Azure for Students` (Değiştirmeyin).
2.  **Resource Group (Kaynak Grubu):**
    *   `Create new` bağlantısına tıklayın.
    *   İsim: `contacts-project-rg` (örneğin).
    *   `OK`'e basın.
3.  **Database details (Veritabanı detayları):**
    *   **Database name:** `free-sql-db-4247763` (Varsayılan kalabilir) veya `ContactsDB`.
    *   **Server (Sunucu):** *En önemli adım burasıdır.*
        *   `Create new` bağlantısına tıklayın.
        *   **Server name:** Benzersiz bir isim girin (örn: `contacts-server-omer-2024`). Yanında yeşil tik ✅ çıkmalı.
        *   **Location:** `(Europe) West Europe` veya `North Europe` seçin (Türkiye'ye yakın).
        *   **Authentication method:** `Use SQL authentication` seçin.
        *   **Server admin login:** Bir kullanıcı adı belirleyin (örn: `adminuser`).
        *   **Password:** Güçlü bir şifre belirleyin. **Bu şifreyi bir yere not edin!**
        *   `OK`'e basarak sunucu ayarlarını kaydedin.
4.  **Oluşturma:**
    *   Sayfanın altındaki `Review + create` butonuna basın.
    *   Ayarları doğruladıktan sonra `Create` butonuna basarak kurulumu başlatın. (Birkaç dakika sürebilir).

## 2. Güvenlik Duvarı Ayarları (Firewall Rules)

**⚠️ Bu adımı yapmazsanız uygulama veritabanına bağlanamaz!**

1.  Veritabanı oluşturulduktan sonra `Go to resource` (Kaynağa git) butonuna basın.
2.  Üst veya sol menüden **"Set server firewall"** (veya **Networking**) seçeneğine tıklayın.
3.  Sayfanın alt kısmında:
    *   **"Allow Azure services and resources to access this server"** -> `YES` olarak işaretleyin. (Render.com bağlantısı için gerekli).
4.  Aynı sayfada üstte:
    *   **"+ Add current client IP address"** butonuna tıklayın. (Kendi bilgisayarınızdan bağlanmak için gerekli).
5.  Sol üst köşeden **Save** (Kaydet) butonuna basın.

## 3. Bağlantı Cümlesi (Connection String)

1.  Veritabanı ana sayfasına geri dönün.
2.  Sol menüden **Settings** > **Connection strings**'e tıklayın.
3.  **ADO.NET** sekmesindeki yazıyı kopyalayın.
    *   Şuna benzer bir yazı olacak:
        `Server=tcp:contacts-server-omer-2024.database.windows.net,1433;Initial Catalog=ContactsDB;Persist Security Info=False;User ID=adminuser;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
    *   İçindeki `{your_password}` kısmını kendi belirlediğiniz şifreyle değiştirmemiz gerekecek.

Bu adımları tamamladığınızda projenizi bu veritabanına bağlamaya hazırız!
