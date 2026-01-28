# RSS Feed Kullanım Kılavuzu

## 📰 RSS Kaynakları Hakkında

BorsaPortal, Türkiye'deki güvenilir ekonomi ve finans haber kaynaklarından otomatik olarak haber çekebilir.

## ✅ Doğrulanmış RSS Kaynakları

Aşağıdaki RSS feed URL'leri test edilmiş ve çalışır durumdadır:

### Genel Ekonomi Haberleri
- **NTV Ekonomi**: `https://www.ntv.com.tr/ekonomi.rss`
- **Habertürk Ekonomi**: `https://www.haberturk.com/rss/ekonomi.xml`
- **Dünya Gazetesi**: `https://www.dunya.com/rss?dunya`
- **Bloomberg HT**: `https://www.bloomberght.com/rss`

### Borsa ve Finans
- **Bigpara** (Hürriyet): `https://bigpara.hurriyet.com.tr/rss/`
- **Investing.com Forex**: `https://tr.investing.com/rss/forex.rss`
- **Investing.com Hisse**: `https://tr.investing.com/rss/stock.rss`

## 🔧 Nasıl Çalışır?

### 1. RSS Feed İşleme Özellikleri

RssFeedService aşağıdaki formatları destekler:

#### Resim Çıkarma (Öncelik Sırasına Göre)
1. **Enclosure Tag** - Standart RSS resim formatı
2. **Media RSS Extension** - `media:content` ve `media:thumbnail`
3. **HTML Parse** - İçerikte bulunan `<img>` tagları

#### Desteklenen RSS Formatları
- RSS 2.0
- Atom
- Media RSS Extensions

### 2. Otomatik Özellikler

- ✅ **Duplicate Kontrolü**: Aynı haber iki kez eklenmez (URL ve başlık bazlı)
- ✅ **Şirket Eşleştirme**: Haber içeriğinde şirket adı/kodu bulunursa otomatik eşleştirilir
- ✅ **Sektör Eşleştirme**: İçerik analizi ile sektör otomatik belirlenir
- ✅ **Resim Çıkarma**: 3 farklı yöntemle otomatik resim bulma
- ✅ **Türkçe Karakter Desteği**: UTF-8 encoding ile tam destek
- ✅ **Filtre Desteği**: Include/Exclude keywords ile içerik filtreleme

### 3. Hata Yönetimi

Sistem aşağıdaki hataları yakalar ve kullanıcıya bilgi verir:

- **Network Hatası**: Bağlantı sorunları
- **Timeout**: 30 saniye zaman aşımı koruması
- **XML Parse Hatası**: Geçersiz RSS formatı
- **HTTP Hataları**: 403, 404, 500 vb. durum kodları

## 📝 Veritabanı Kurulumu

SQL script'i çalıştırarak RSS kaynaklarını ekleyin:

```bash
# SQL Server'da çalıştırın
sqlcmd -S localhost -d BorsaPortalDB -i CreateRssFeedSourcesTable.sql
```

Veya Entity Framework Migration kullanın:

```bash
Update-Database
```

## 🎯 Kullanım

### Admin Panelinden

1. `/Admin/NewsAutomation` sayfasına gidin
2. RSS kaynaklarını görüntüleyin
3. **Tüm Kaynakları Senkronize Et** butonuna tıklayın
4. Veya tek tek kaynakları senkronize edin

### Özelleştirme

#### Yeni RSS Kaynağı Eklemek

```sql
INSERT INTO RssFeedSources
    (Name, Description, FeedUrl, IsActive, FetchIntervalMinutes, AutoPublish, CreatedAt, IsDeleted)
VALUES
    ('Kaynak Adı', 'Açıklama', 'https://example.com/rss', 1, 30, 0, GETDATE(), 0)
```

#### Filtre Eklemek

```sql
-- Sadece "borsa" ve "hisse" içeren haberleri çek
UPDATE RssFeedSources
SET IncludeKeywords = 'borsa,hisse,BIST'
WHERE Id = 1

-- "politika" içeren haberleri çekme
UPDATE RssFeedSources
SET ExcludeKeywords = 'politika,seçim'
WHERE Id = 1
```

## ⚙️ Ayarlar

- **FetchIntervalMinutes**: Kaynağın kaç dakikada bir kontrol edileceği (önerilen: 30-60)
- **AutoPublish**: Gelen haberlerin otomatik yayınlanıp yayınlanmayacağı (0: Manuel onay gerekir)
- **SectorId**: Kaynak belirli bir sektöre ait ise sektör ID'si
- **IsActive**: Kaynağın aktif olup olmadığı

## 🐛 Sorun Giderme

### Haber Gelmiyor
1. RSS URL'sinin doğru ve erişilebilir olduğunu kontrol edin
2. `IsActive = 1` olduğundan emin olun
3. `LastFetchedAt` zamanına bakın - yakın zamanda çekilmiş mi?

### Resimler Gelmiyor
- Feed kaynağı resim içermiyor olabilir
- `ImageUrl` alanını manuel doldurmayı deneyin
- Media RSS extension desteği olmayan feed'ler için normal

### Timeout Hataları
- `FetchIntervalMinutes` değerini artırın
- Yavaş feed'ler için 60+ dakika kullanın

## 📚 Ek Kaynaklar

### Diğer Kullanılabilir RSS Feed'ler

```
Forbes Türkiye: https://www.forbes.com.tr/rss
Ekonomim: https://www.ekonomim.com/export/rss
AA Ekonomi: https://www.aa.com.tr/tr/rss/default?cat=ekonomi
```

### RSS Feed Bulma İpuçları

1. Haber sitesine gidin
2. Sayfanın alt kısmında "RSS" linkine bakın
3. Veya `/rss`, `/feed`, `/rss.xml` URL'lerini deneyin
4. Tarayıcı geliştirici araçlarında `<link type="application/rss+xml">` tagını arayın

## 🔐 Güvenlik

- Anti-forgery token koruması aktif
- Sadece Admin rolü senkronizasyon yapabilir
- SQL injection koruması mevcut
- XSS koruması için HTML encode edilir

## 📊 Performans

- Static HttpClient kullanımı (socket exhaustion önlenir)
- 30 saniye timeout
- Batch işlemler için optimizasyon
- Duplicate kontrolü ile gereksiz kayıt önlenir

---

**Son Güncelleme**: 2026-01-20
**Versiyon**: 1.0
