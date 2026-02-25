# Deployment Rehberi

Projenizi ücretsiz olarak yayına almak için aşağıdaki adımları takip edin.

## 1. Hazırlık
1.  Bu kodları GitHub'a pushlayın:
    ```bash
    git add .
    git commit -m "Deployment hazırlığı"
    git push
    ```

## 2. Backend Deployment (Render.com)
1.  [Render Dashboard](https://dashboard.render.com/)'a gidin ve önce **"New + -> PostgreSQL"** seçin.
2.  PostgreSQL servisini oluşturun (`Name: contacts-db`, `Database: contactsarge24`, `User: contactsuser` gibi).
3.  PostgreSQL oluşturulunca **External Database URL** veya **Connection String** bilgisini kopyalayın.
4.  Ardından **"New + -> Web Service"** seçin.
5.  GitHub hesabınızı bağlayın ve bu projeyi seçin.
6.  Ayarları şöyle yapın:
    *   **Name:** `contacts-api` (veya istediğiniz bir isim)
    *   **Runtime:** `Docker`
    *   **Region:** `Frankfurt` (veya size yakın olan)
    *   **Branch:** `main` (veya `master`)
    *   **Root Directory:** (Boş bırakın - böylece proje ana dizini Build Context olur)
    *   **Dockerfile Path:** `./src/backend/Contacts.Api/Dockerfile` (Çok Önemli! Dockerfile'ı buradan bulacak)
7.  Sayfanın altındaki **"Environment Variables"** kısmına şu değişkenleri ekleyin (önce `Add Environment Variable`'a basın):
    *   **Key:** `ConnectionStrings__DefaultConnection` -> **Value:** (Render PostgreSQL connection string'inizi buraya yapıştırın)
    *   **Key:** `ASPNETCORE_ENVIRONMENT` -> **Value:** `Production`
    *   **Key:** `ASPNETCORE_URLS` -> **Value:** `http://+:5050`
8.  **"Create Web Service"** butonuna basın.
9.  Deployment bitince size `https://contacts-api-xxxx.onrender.com` gibi bir URL verecek. **Bu URL'i kopyalayın.**

## 3. Frontend Konfigürasyonu (Build URL'i Ekleme)
1.  Kendi bilgisayarınızda `src/frontend/contacts-web/src/environments/environment.prod.ts` dosyasını açın.
2.  `apiUrl` kısmına az önce kopyaladığınız Render URL'ini yapıştırın (sonuna `/api` eklemeyi unutmayın - dikkat edin `/api` yolunu projenize göre ayarlayın, benim yazdığım kodda `/api` base path değildir, controllerlar `/api/contacts` gibi başlıyorsa sadece domaini yazın. Eğer `baseUrl` kodda `/api` ile başlıyorsa, buraya `https://domain.com` yazıp kodda `/api` eklendiğinden emin olun.
    *   **Düzeltme:** Kodda `contacts-api.service.ts`: `baseUrl = environment.apiUrl + '/contacts'`.
    *   Yani `environment.prod.ts` içindeki `apiUrl` sadece `https://contacts-api-xxxx.onrender.com/api` olmalı. (Çünkü service `/contacts` ekliyor).
    ```typescript
    export const environment = {
      production: true,
      apiUrl: 'https://SİZİN-RENDER-URLNİZ.onrender.com/api' // Sonunda /api olsun
    };
    ```
3.  Değişikliği kaydedip GitHub'a gönderin:
    ```bash
    git add .
    git commit -m "Backend URL güncellendi"
    git push
    ```

## 4. Frontend Deployment (Vercel)
1.  [Vercel Dashboard](https://vercel.com/)'a gidin ve **"Add New Project"** deyin.
2.  GitHub'dan bu projeyi import edin.
3.  **Framework Preset:** `Angular` otomatik seçilmeli.
4.  **Root Directory:** `Edit` butonuna basın ve `src/frontend/contacts-web` klasörünü seçin.
5.  **Deploy** butonuna basın.
6.  Deployment bitince size `https://contacts-app.vercel.app` gibi bir URL verecek.

## 5. Son Rötuş (CORS Ayarı)
Backend'in, Frontend'den gelen isteklere izin vermesi için son bir ayar yapmamız lazım.

1.  Render Dashboard'a geri dönün -> Backend servisine girin -> **Environment** sekmesi.
2.  Yeni bir değişken ekleyin:
    *   **Key:** `Cors__AllowedOrigins__0` (Sıfır rakamı var sonda)
    *   **Value:** Vercel'in size verdiği URL (örn: `https://contacts-app.vercel.app` - sonunda slash `/` olmasın).
3.  **Save Changes** diyin. Render otomatik yeniden başlayacak.

🎉 **Tebrikler! Projeniz artık canlıda.**
