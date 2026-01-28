# Yahoo Finance API Kullanımı

## Yahoo Finance API Nedir?

Yahoo Finance, dünya genelinde hisse senedi, döviz, emtia ve endeks verilerini ücretsiz olarak sağlayan bir finansal veri kaynağıdır. BorsaPortal projesi, Yahoo Finance'in genel API endpoint'lerini kullanarak BIST (Borsa İstanbul) ve diğer piyasa verilerini çekmektedir.

## Özellikler

- ✅ **Ücretsiz** - API key gerektirmez
- ✅ **BIST Desteği** - Tüm BIST hisseleri desteklenir
- ✅ **Gerçek Zamanlı Veriler** - Güncel fiyat, değişim ve hacim verileri
- ✅ **Geçmiş Veriler** - Tarihsel fiyat verileri
- ✅ **Döviz ve Emtialar** - USD/TRY, EUR/TRY, altın, vb.
- ✅ **Kurulum Gerektirmez** - Hazır çalışır durumda

## Kullanılan Veri Kaynakları

BorsaPortal aşağıdaki Yahoo Finance endpoint'lerini kullanır:

### 1. Ana Sayfa Piyasa Verileri
- **BIST 100**: `XU100.IS`
- **USD/TRY**: `USDTRY=X`
- **EUR/TRY**: `EURTRY=X`
- **Altın**: `GC=F` (Gold Futures)

### 2. BIST Hisse Verileri
BIST hisseleri için `.IS` suffix kullanılır:
- **Türk Hava Yolları**: `THYAO.IS`
- **Akbank**: `AKBNK.IS`
- **Garanti Bankası**: `GARAN.IS`

## Sembol Formatı

Yahoo Finance'de BIST hisseleri için sembol formatı:
```
[HİSSE_KODU].IS
```

Örnekler:
- THYAO → `THYAO.IS`
- ASELS → `ASELS.IS`
- EREGL → `EREGL.IS`

## API Endpoint'leri

### Güncel Fiyat ve Piyasa Verileri
```
https://query1.finance.yahoo.com/v8/finance/chart/[SYMBOL]?interval=1d&range=1d
```

### Geçmiş Veriler
```
https://query1.finance.yahoo.com/v8/finance/chart/[SYMBOL]?interval=1d&range=[5d|1mo|3mo|6mo|1y]
```

## Kullanılan Servisler

### 1. MarketDataService
Ana sayfa piyasa özetini sağlar:
- BIST 100 endeksi
- Dolar/TL ve Euro/TL kurları
- Gram altın fiyatı

### 2. StockPriceService
Portföy yönetimi için hisse fiyatlarını sağlar:
- Tekil hisse fiyatı
- Çoklu hisse fiyatları
- Portföy güncellemeleri
- Geçmiş fiyat verileri

## Rate Limiting

Yahoo Finance resmi bir rate limit belirtmemiştir, ancak makul kullanım için:
- İstekler arası **200ms** bekleme süresi
- Toplu istekler için sıralı işleme
- Başarısız istekler için otomatik retry yok

## Sorun Giderme

### Veri Gelmiyor
- İnternet bağlantınızı kontrol edin
- Yahoo Finance'in erişilebilir olduğunu doğrulayın
- Hisse kodu formatının doğru olduğundan emin olun (`.IS` suffix)

### Yanlış Fiyatlar
- Yahoo Finance bazen 15-20 dakika gecikme ile veri sağlar
- BIST piyasa saatleri dışında son kapanış fiyatları gösterilir

### "Chart data is null" hatası
- Hisse kodunun Yahoo Finance'de mevcut olduğunu doğrulayın
- `.IS` suffix'inin eklendiğinden emin olun
- Bazı eski veya düşük hacimli hisseler mevcut olmayabilir

## Test Etme

Projeyi çalıştırdıktan sonra:

1. **Ana Sayfa**
   - BIST 100, döviz ve altın verileri otomatik yüklenir
   - Her 30 saniyede bir güncellenir

2. **Portföy Sayfası**
   - Portföyünüze hisse ekleyin
   - Fiyatlar otomatik olarak güncellenir
   - "Fiyatları Güncelle" butonu ile manuel güncelleme

## Alternatifler

Yahoo Finance kullanılamıyorsa alternatif veri kaynakları:
- **Alpha Vantage** - Ücretsiz API key gerektirir
- **Investing.com** - Web scraping gerektirir
- **BIST Resmi API** - Ücretli ve kurumsal

## Notlar

- Yahoo Finance resmi bir API değildir ve değişebilir
- Üretim ortamında daha güvenilir ücretli API'ler tercih edilebilir
- Veriler "olduğu gibi" sağlanır, doğruluğu garanti edilmez
- Bu proje Yahoo Finance ile ilişkili değildir

---

**Son Güncelleme:** 2026-01-28
**Durum:** Aktif ve çalışıyor ✅
