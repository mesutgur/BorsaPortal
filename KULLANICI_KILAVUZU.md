# Borsa Portal Kullanıcı Kılavuzu

## 🌟 Yeni Özellikler

### 1. Dark Mode (Karanlık Tema)
Gözlerinizi yormayan karanlık tema desteği eklendi.

**Nasıl Kullanılır:**
- Navbar'daki ay/güneş ikonuna tıklayın
- Klavye kısayolu: `Alt + D`
- Tema tercihiniz tarayıcınızda saklanır

**Avantajlar:**
- Gece kullanımında göz yorgunluğunu azaltır
- Düşük ışık ortamlarında daha rahat kullanım
- Pil ömründen tasarruf (OLED ekranlarda)

---

### 2. Toast Bildirimleri
Modern, zarif bildirim sistemi.

**Özellikler:**
- Başarılı işlemler: Yeşil bildirim
- Hatalar: Kırmızı bildirim
- Bilgi mesajları: Mavi bildirim
- Otomatik kapanma (3 saniye)
- Ekranın sağ üst köşesinde görünür

**Kullanım:**
Form gönderimlerinde, API çağrılarında ve önemli işlemlerde otomatik olarak görünür.

---

### 3. Klavye Kısayolları
Hızlı erişim için klavye kısayolları:

| Kısayol | İşlev |
|---------|-------|
| `Alt + D` | Karanlık/Aydınlık tema değiştir |
| `Alt + N` | Bildirimlere git |
| `Alt + P` | Portföye git |
| `Alt + H` | Ana sayfaya git |

---

### 4. Gelişmiş Analiz Araçları

#### 4.1 Çeşitlendirme Skoru
Portföyünüzün ne kadar çeşitlendirilmiş olduğunu 0-100 arası bir skorla değerlendirir.

**Bileşenler:**
- **HHI İndeksi**: Piyasa konsantrasyonu ölçümü
- **Hisse Sayısı**: Portföyünüzdeki farklı hisse sayısı
- **Sektör Sayısı**: Yatırım yaptığınız sektör sayısı
- **İlk 5 Konsantrasyonu**: En büyük 5 hissenizin toplam portföy içindeki oranı

**Skor Değerlendirmesi:**
- 70-100: Mükemmel çeşitlendirme
- 50-69: İyi, iyileştirilebilir
- 0-49: Yetersiz, risk yüksek

#### 4.2 Risk Metrikleri
**Volatilite**: Getirilerinizin ne kadar dalgalı olduğunu gösterir
**Sharpe Oranı**: Risk-getiri dengesini ölçer
**Maksimum Düşüş**: En kötü performans gösteren hisseniz

**Risk Seviyeleri:**
- Düşük: Volatilite < 10%
- Orta: Volatilite 10-20%
- Yüksek: Volatilite 20-30%
- Çok Yüksek: Volatilite > 30%

#### 4.3 Hisse Karşılaştırma
**Özellikler:**
- 2-5 hisse arasında karşılaştırma
- Yan yana performans grafikleri
- Detaylı metrik tablosu
- Toplam değer ve kar/zarar görünümü

**Nasıl Kullanılır:**
1. Gelişmiş Analiz > Hisse Karşılaştır'a gidin
2. Karşılaştırmak istediğiniz hisseleri seçin
3. "Karşılaştır" butonuna tıklayın

#### 4.4 Performans Trendleri
**Zaman Periyotları:**
- Son 3 Ay
- Son 6 Ay
- Son 1 Yıl
- Son 2 Yıl

**Grafikler:**
- Yatırım vs Güncel Değer trendi
- Aylık Kar/Zarar grafiği
- İşlem hacmi trendi

---

### 5. Bildirim Sistemi

#### 5.1 Hedef Fiyat Bildirimleri
İzleme listenizde hedef fiyat belirlediğiniz hisseler bu fiyata ulaştığında otomatik bildirim alırsınız.

**Özellikler:**
- Günde bir kez kontrol
- Duplikasyon önleme
- Detaylı bildirim mesajı (fiyat, yüzde değişim)

#### 5.2 Bildirim Tercihleri
Bildirim > Ayarlar'dan tercihlerinizi özelleştirin:
- Hedef Fiyat Bildirimleri: Açık/Kapalı
- KAP Bildirimleri: Açık/Kapalı
- Haber Bildirimleri: Açık/Kapalı
- Email Bildirimleri: Açık/Kapalı
- Günlük Özet Saati: Belirleme

---

### 6. Raporlama Özellikleri

#### 6.1 Portföy Performans Raporu
**İçerik:**
- Dönem yatırımı ve getiri analizi
- Sektör dağılımı pasta grafiği
- Detaylı işlem listesi
- Tüm portföy özet istatistikleri

**Export Seçenekleri:**
- Yazdırma desteği
- Gelecekte: PDF ve Excel export

#### 6.2 İşlem Geçmişi Raporu
**Filtreler:**
- Tarih aralığı seçimi
- Şirkete göre filtreleme

**Özellikler:**
- CSV export desteği
- Özet istatistik kartları
- Detaylı işlem tablosu

---

## 🎨 Tasarım ve Erişilebilirlik

### Responsive Tasarım
- Mobil uyumlu (col-6 col-lg-3 grid sistemi)
- Tablet ve desktop için optimize
- Esnek kart düzenleri

### Erişilebilirlik
- ARIA etiketleri
- Klavye navigasyonu
- Fokus göstergeleri (`outline: 2px solid #0d6efd`)
- Ekran okuyucu uyumluluğu

### Görsel İyileştirmeler
- Smooth transitions (0.3s ease)
- Hover efektleri
- Color-coded indicators (yeşil=kazanç, kırmızı=kayıp)
- Progress bar'lar ve badge'ler

---

## 💡 İpuçları

### Çeşitlendirme
- En az 10 farklı hisse bulundurun
- 4-6 farklı sektöre yayın
- İlk 5 hisseniz portföyünüzün %50'sini geçmesin

### Risk Yönetimi
- Volatiliteyi düzenli kontrol edin
- Sharpe oranını izleyin (yüksek olanı tercih edin)
- Stop-loss için hedef fiyat belirleyin

### Bildirimler
- Gereksiz bildirimlerden kaçının
- Günlük özet özelliğini kullanın
- Önemli KAP bildirimlerini takip edin

### Performans Takibi
- Aylık performansınızı düzenli inceleyin
- Trend grafiklerini kullanın
- Karşılaştırma özelliğiyle hisselerinizi analiz edin

---

## 🔧 Teknik Özellikler

### API Entegrasyonu
- Yahoo Finance API ile gerçek zamanlı BIST fiyatları
- Rate limiting (500ms delay)
- User-Agent header ile bot koruması

### Veri Güvenliği
- AntiForgeryToken ile CSRF koruması
- Soft delete pattern
- User-specific data isolation

### Performans
- AJAX ile asenkron veri yükleme
- Eager loading (Entity Framework Include)
- Client-side caching (localStorage)

---

## 📞 Destek

Sorun yaşarsanız veya önerileriniz varsa:
- GitHub Issues: https://github.com/anthropics/claude-code/issues
- Email: info@borsaportal.com
- Telefon: +90 (212) 000 00 00

---

**Son Güncelleme:** 2026-01-17
**Versiyon:** 1.0.0
