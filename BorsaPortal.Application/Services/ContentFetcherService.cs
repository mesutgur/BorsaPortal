using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BorsaPortal.Application.Services
{
    /// <summary>
    /// Web sayfalarından haber içeriği çeken servis
    /// </summary>
    public class ContentFetcherService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static ContentFetcherService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
        }

        /// <summary>
        /// Verilen URL'den haber içeriğini çeker ve parse eder
        /// </summary>
        public async Task<FetchedContent> FetchContentFromUrlAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("URL boş olamaz.", nameof(url));
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("Geçersiz URL formatı.", nameof(url));
            }

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Web sayfası okunamadı. HTTP Status: {response.StatusCode}");
                }

                var html = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(html))
                {
                    throw new Exception("Web sayfası içeriği boş.");
                }

                return ParseHtmlContent(html, url);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Web sayfasına bağlanırken hata oluştu: {ex.Message}", ex);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("İstek zaman aşımına uğradı. Lütfen daha sonra tekrar deneyin.");
            }
        }

        /// <summary>
        /// HTML içeriğini parse eder ve ana metin içeriğini çıkarır
        /// </summary>
        private FetchedContent ParseHtmlContent(string html, string sourceUrl)
        {
            var result = new FetchedContent
            {
                SourceUrl = sourceUrl
            };

            try
            {
                // Meta etiketlerden başlık ve açıklama çek
                result.Title = ExtractMetaContent(html, "og:title")
                    ?? ExtractMetaContent(html, "twitter:title")
                    ?? ExtractTitleTag(html)
                    ?? "";

                result.Summary = ExtractMetaContent(html, "og:description")
                    ?? ExtractMetaContent(html, "twitter:description")
                    ?? ExtractMetaContent(html, "description")
                    ?? "";

                result.ImageUrl = ExtractMetaContent(html, "og:image")
                    ?? ExtractMetaContent(html, "twitter:image")
                    ?? ExtractFirstImage(html)
                    ?? "";

                // Ana içeriği çıkar - yaygın HTML yapılarını dene
                result.Content = ExtractMainContent(html);

                // İçerik çok kısaysa meta description'ı kullan
                if (string.IsNullOrWhiteSpace(result.Content) || result.Content.Length < 100)
                {
                    result.Content = result.Summary;
                }

                // Başlık yoksa title tag'i kullan
                if (string.IsNullOrWhiteSpace(result.Title))
                {
                    result.Title = ExtractTitleTag(html) ?? "Başlık Bulunamadı";
                }

                // Başlığı temizle (site adını kaldır)
                result.Title = CleanTitle(result.Title);

                // Summary yoksa içeriğin ilk kısmını al
                if (string.IsNullOrWhiteSpace(result.Summary) && !string.IsNullOrWhiteSpace(result.Content))
                {
                    result.Summary = TruncateText(StripHtmlTags(result.Content), 300);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"HTML içeriği parse edilirken hata oluştu: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// Meta tag içeriğini çıkarır
        /// </summary>
        private string ExtractMetaContent(string html, string propertyName)
        {
            try
            {
                // property veya name attribute'unu kontrol et
                var patterns = new[]
                {
                    $@"<meta\s+property\s*=\s*['""]({Regex.Escape(propertyName)})['""][^>]*content\s*=\s*['""]([^'""]+)['""]",
                    $@"<meta\s+content\s*=\s*['""]([^'""]+)['""][^>]*property\s*=\s*['""]({Regex.Escape(propertyName)})['""]",
                    $@"<meta\s+name\s*=\s*['""]({Regex.Escape(propertyName)})['""][^>]*content\s*=\s*['""]([^'""]+)['""]",
                    $@"<meta\s+content\s*=\s*['""]([^'""]+)['""][^>]*name\s*=\s*['""]({Regex.Escape(propertyName)})['""]"
                };

                foreach (var pattern in patterns)
                {
                    var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // İlk grup property/name, ikinci grup content
                        var content = match.Groups.Count > 2 ? match.Groups[2].Value : match.Groups[1].Value;
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            return DecodeHtmlEntities(content.Trim());
                        }
                    }
                }
            }
            catch
            {
                // Meta tag parse hatası - devam et
            }

            return null;
        }

        /// <summary>
        /// Title tag içeriğini çıkarır
        /// </summary>
        private string ExtractTitleTag(string html)
        {
            try
            {
                var match = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success && match.Groups.Count > 1)
                {
                    return DecodeHtmlEntities(match.Groups[1].Value.Trim());
                }
            }
            catch
            {
                // Title tag parse hatası
            }

            return null;
        }

        /// <summary>
        /// İlk img tag'inden resim URL'sini çıkarır
        /// </summary>
        private string ExtractFirstImage(string html)
        {
            try
            {
                var match = Regex.Match(html, @"<img[^>]+src\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1)
                {
                    var url = match.Groups[1].Value;
                    if (url.StartsWith("http") || url.StartsWith("//"))
                    {
                        if (url.StartsWith("//"))
                        {
                            url = "https:" + url;
                        }
                        return url;
                    }
                }
            }
            catch
            {
                // Image extraction hatası
            }

            return null;
        }

        /// <summary>
        /// Ana içeriği HTML'den çıkarır
        /// </summary>
        private string ExtractMainContent(string html)
        {
            var content = new StringBuilder();

            try
            {
                // Yaygın haber sitesi içerik etiketlerini dene
                var contentPatterns = new[]
                {
                    @"<article[^>]*>(.*?)</article>",
                    @"<div[^>]*class\s*=\s*[""'][^""']*article[^""']*[""'][^>]*>(.*?)</div>",
                    @"<div[^>]*class\s*=\s*[""'][^""']*content[^""']*[""'][^>]*>(.*?)</div>",
                    @"<div[^>]*class\s*=\s*[""'][^""']*entry-content[^""']*[""'][^>]*>(.*?)</div>",
                    @"<div[^>]*class\s*=\s*[""'][^""']*post-content[^""']*[""'][^>]*>(.*?)</div>",
                    @"<div[^>]*id\s*=\s*[""']content[""'][^>]*>(.*?)</div>",
                    @"<main[^>]*>(.*?)</main>"
                };

                foreach (var pattern in contentPatterns)
                {
                    var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        var extractedContent = match.Groups[1].Value;

                        // <p> taglarını çıkar
                        var paragraphs = Regex.Matches(extractedContent, @"<p[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        if (paragraphs.Count > 0)
                        {
                            foreach (Match p in paragraphs)
                            {
                                if (p.Groups.Count > 1)
                                {
                                    var text = CleanHtmlContent(p.Groups[1].Value);
                                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 20)
                                    {
                                        content.AppendLine($"<p>{text}</p>");
                                    }
                                }
                            }

                            if (content.Length > 100)
                            {
                                return content.ToString();
                            }
                        }
                    }
                }

                // Eğer yukarıdaki pattern'ler çalışmazsa, tüm <p> taglarını topla
                if (content.Length == 0)
                {
                    var allParagraphs = Regex.Matches(html, @"<p[^>]*>(.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    int paragraphCount = 0;

                    foreach (Match p in allParagraphs)
                    {
                        if (p.Groups.Count > 1 && paragraphCount < 20) // İlk 20 paragrafı al
                        {
                            var text = CleanHtmlContent(p.Groups[1].Value);
                            if (!string.IsNullOrWhiteSpace(text) && text.Length > 30)
                            {
                                content.AppendLine($"<p>{text}</p>");
                                paragraphCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // İçerik çıkarma hatası
                throw new Exception($"Ana içerik çıkarılamadı: {ex.Message}", ex);
            }

            return content.ToString();
        }

        /// <summary>
        /// HTML içeriğini temizler (script, style vs. kaldırır)
        /// </summary>
        private string CleanHtmlContent(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Script ve style taglarını kaldır
            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // HTML entity'leri decode et
            html = DecodeHtmlEntities(html);

            // Birden fazla boşluğu tek boşluğa indir
            html = Regex.Replace(html, @"\s+", " ");

            return html.Trim();
        }

        /// <summary>
        /// HTML taglarını tamamen kaldırır
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Tüm HTML taglarını kaldır
            var text = Regex.Replace(html, @"<[^>]+>", " ");

            // HTML entity'leri decode et
            text = DecodeHtmlEntities(text);

            // Birden fazla boşluğu tek boşluğa indir
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }

        /// <summary>
        /// HTML entity'leri decode eder
        /// </summary>
        private string DecodeHtmlEntities(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Replace("&nbsp;", " ")
                      .Replace("&quot;", "\"")
                      .Replace("&apos;", "'")
                      .Replace("&lt;", "<")
                      .Replace("&gt;", ">")
                      .Replace("&amp;", "&")
                      .Replace("&#39;", "'")
                      .Replace("&#34;", "\"")
                      .Replace("&ndash;", "–")
                      .Replace("&mdash;", "—")
                      .Replace("&hellip;", "…");

            // Numeric entities (&#xxx;)
            text = Regex.Replace(text, @"&#(\d+);", match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int charCode))
                {
                    return ((char)charCode).ToString();
                }
                return match.Value;
            });

            return text;
        }

        /// <summary>
        /// Başlığı temizler (site adını vs. kaldırır)
        /// </summary>
        private string CleanTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return title;

            // Yaygın ayırıcıları kullanarak site adını kaldır
            var separators = new[] { " | ", " - ", " – ", " — ", " » " };

            foreach (var separator in separators)
            {
                var parts = title.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    // İlk parça genellikle asıl başlıktır
                    return parts[0].Trim();
                }
            }

            return title.Trim();
        }

        /// <summary>
        /// Metni belirtilen uzunlukta keser
        /// </summary>
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength).TrimEnd() + "...";
        }
    }

    /// <summary>
    /// Çekilen içerik modeli
    /// </summary>
    public class FetchedContent
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string SourceUrl { get; set; }
    }
}
