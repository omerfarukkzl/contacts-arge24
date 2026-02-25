# Azure SQL Setup (Deprecated)

Bu proje artık Azure SQL kullanmıyor; veritabanı katmanı PostgreSQL'e taşındı.

- Production için: `docs/DEPLOYMENT_GUIDE.md` içindeki Render PostgreSQL adımlarını izleyin.
- Local için: `docker compose up -d postgres` komutunu kullanın.
- API connection string formatı:
  `Host=localhost;Port=5432;Database=ContactsArge24;Username=postgres;Password=postgres`
