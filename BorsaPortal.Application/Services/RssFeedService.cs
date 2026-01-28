using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using BorsaPortal.Domain.Entities;

namespace BorsaPortal.Application.Services
{
    /// <summary>
    /// RSS feed okuma ve parse etme servisi
    /// </summary>
    public class RssFeedService : IRssFeedService
    {
        // Static HttpClient to avoid socket exhaustion and improve performance
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static RssFeedService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BorsaPortal/1.0 RSS Reader");
        }

        public RssFeedService()
        {
            // Constructor intentionally left empty - using static HttpClient
        }

        /// <summary>
        /// RSS feed URL'inden haberleri çeker ve parse eder
        /// </summary>
        public async Task<List<RssFeedItem>> FetchFeedItemsAsync(string feedUrl)
        {
            var items = new List<RssFeedItem>();

            try
            {
                var response = await _httpClient.GetAsync(feedUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"RSS feed okunamadı. HTTP Status: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var stream = await response.Content.ReadAsStreamAsync();

                // XML okuma ayarları - daha toleranslı
                var xmlReaderSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    CloseInput = false,
                    // Hatalı karakterleri görmezden gel
                    CheckCharacters = false
                };

                // İlk önce SyndicationFeed ile dene, başarısız olursa manuel parse et
                try
                {
                    stream.Position = 0; // Stream'i başa al
                    using (var xmlReader = XmlReader.Create(stream, xmlReaderSettings))
                    {
                        var feed = SyndicationFeed.Load(xmlReader);

                        if (feed?.Items == null)
                        {
                            throw new Exception("RSS feed parse edildi ancak haber öğeleri bulunamadı.");
                        }

                        foreach (var feedItem in feed.Items)
                        {
                            items.Add(ParseSyndicationItem(feedItem));
                        }
                    }
                }
                catch (Exception ex) when (ex is XmlException || ex is FormatException || ex is InvalidOperationException)
                {
                    // SyndicationFeed.Load başarısız oldu, manuel parsing dene
                    try
                    {
                        stream.Position = 0; // Stream'i tekrar başa al
                        items = ParseRssFeedManually(stream);
                    }
                    catch (Exception manualEx)
                    {
                        // Manuel parsing de başarısız oldu, orijinal hatayı fırlat
                        throw new XmlException($"RSS feed okunamadı. SyndicationFeed hatası: {ex.Message}. Manuel parsing hatası: {manualEx.Message}", ex);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"RSS feed'e bağlanırken ağ hatası oluştu: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("RSS feed okuma işlemi zaman aşımına uğradı (30 saniye). Lütfen daha sonra tekrar deneyin.", ex);
            }
            catch (XmlException ex)
            {
                throw new Exception($"RSS feed XML formatı hatalı: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Eğer yukarıdaki özel catch'lerden biri değilse, genel hata mesajı ile fırlat
                if (ex.Message.StartsWith("RSS feed"))
                {
                    throw; // Kendi attığımız hatayı tekrar fırlat
                }
                throw new Exception($"RSS feed işlenirken beklenmeyen hata: {ex.Message}", ex);
            }

            return items;
        }

        /// <summary>
        /// RSS feed source'undan haberleri çeker
        /// </summary>
        public async Task<List<RssFeedItem>> FetchFeedItemsAsync(RssFeedSource feedSource)
        {
            if (feedSource == null || string.IsNullOrEmpty(feedSource.FeedUrl))
                return new List<RssFeedItem>();

            var items = await FetchFeedItemsAsync(feedSource.FeedUrl);

            // Apply filters if configured
            if (!string.IsNullOrEmpty(feedSource.IncludeKeywords))
            {
                var includeKeywords = feedSource.IncludeKeywords
                    .Split(',')
                    .Select(k => k.Trim().ToLower())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                if (includeKeywords.Any())
                {
                    items = items.Where(item =>
                    {
                        var searchText = $"{item.Title} {item.Summary} {item.Content}".ToLower();
                        return includeKeywords.Any(keyword => searchText.Contains(keyword));
                    }).ToList();
                }
            }

            if (!string.IsNullOrEmpty(feedSource.ExcludeKeywords))
            {
                var excludeKeywords = feedSource.ExcludeKeywords
                    .Split(',')
                    .Select(k => k.Trim().ToLower())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                if (excludeKeywords.Any())
                {
                    items = items.Where(item =>
                    {
                        var searchText = $"{item.Title} {item.Summary} {item.Content}".ToLower();
                        return !excludeKeywords.Any(keyword => searchText.Contains(keyword));
                    }).ToList();
                }
            }

            return items;
        }

        /// <summary>
        /// SyndicationItem'i RssFeedItem'a dönüştürür
        /// </summary>
        private RssFeedItem ParseSyndicationItem(SyndicationItem feedItem)
        {
            // PublishDate'i güvenli şekilde al
            DateTime publishDate = DateTime.Now;
            try
            {
                if (feedItem.PublishDate.DateTime != DateTime.MinValue)
                {
                    publishDate = feedItem.PublishDate.DateTime;
                }
                else if (feedItem.LastUpdatedTime.DateTime != DateTime.MinValue)
                {
                    publishDate = feedItem.LastUpdatedTime.DateTime;
                }
            }
            catch
            {
                // DateTime parse hatası - varsayılan değer kullan
                publishDate = DateTime.Now;
            }

            var item = new RssFeedItem
            {
                Title = feedItem.Title?.Text ?? "",
                Link = feedItem.Links?.FirstOrDefault()?.Uri?.ToString() ?? "",
                PublishDate = publishDate,
                Author = feedItem.Authors?.FirstOrDefault()?.Name ?? "",
                Categories = feedItem.Categories?.Select(c => c.Name).ToList() ?? new List<string>()
            };

            // Summary (description) - güvenli şekilde al
            try
            {
                if (feedItem.Summary?.Text != null)
                {
                    item.Summary = feedItem.Summary.Text;
                }
            }
            catch
            {
                // Summary parse hatası - boş bırak
                item.Summary = "";
            }

            // Content (full text if available) - güvenli şekilde al
            // Öncelikle content:encoded gibi extension'ları kontrol et
            string encodedContent = null;
            try
            {
                foreach (var ext in feedItem.ElementExtensions)
                {
                    if (ext.OuterName.Equals("encoded", StringComparison.OrdinalIgnoreCase) ||
                        ext.OuterName.Equals("content:encoded", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            encodedContent = ext.GetObject<string>();
                            if (!string.IsNullOrEmpty(encodedContent))
                            {
                                break;
                            }
                        }
                        catch
                        {
                            // Extension parse hatası - devam et
                        }
                    }
                }
            }
            catch
            {
                // Extension hatası - devam et
            }

            // Content'i belirle: encoded > content > summary
            try
            {
                if (!string.IsNullOrEmpty(encodedContent))
                {
                    item.Content = encodedContent;
                    // Eğer summary boşsa, content'in ilk kısmını al
                    if (string.IsNullOrEmpty(item.Summary))
                    {
                        item.Summary = TruncateHtml(encodedContent, 500);
                    }
                }
                else
                {
                    var content = feedItem.Content as TextSyndicationContent;
                    if (content?.Text != null)
                    {
                        item.Content = content.Text;
                    }
                    else
                    {
                        // Fallback to summary
                        item.Content = item.Summary;
                    }
                }
            }
            catch
            {
                // Content parse hatası - summary kullan
                item.Content = item.Summary;
            }

            // Extract image from multiple sources (priority order)
            item.ImageUrl = ExtractImageUrl(feedItem, item.Content, item.Summary);

            return item;
        }

        /// <summary>
        /// RSS/Atom feed'i manuel olarak parse eder (SyndicationFeed.Load başarısız olduğunda)
        /// </summary>
        private List<RssFeedItem> ParseRssFeedManually(System.IO.Stream stream)
        {
            var items = new List<RssFeedItem>();

            try
            {
                var doc = XDocument.Load(stream);
                var root = doc.Root;

                if (root == null)
                    return items;

                // RSS 2.0 format kontrolü
                if (root.Name.LocalName == "rss")
                {
                    items = ParseRss20(doc);
                }
                // Atom format kontrolü
                else if (root.Name.LocalName == "feed")
                {
                    items = ParseAtom(doc);
                }
                // RDF (RSS 1.0) format kontrolü
                else if (root.Name.LocalName == "RDF")
                {
                    items = ParseRdf(doc);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Manuel RSS parsing hatası: {ex.Message}", ex);
            }

            return items;
        }

        /// <summary>
        /// RSS 2.0 formatını parse eder
        /// </summary>
        private List<RssFeedItem> ParseRss20(XDocument doc)
        {
            var items = new List<RssFeedItem>();

            var itemElements = doc.Descendants("item");

            foreach (var itemElement in itemElements)
            {
                var item = new RssFeedItem();

                // Title
                item.Title = GetElementValue(itemElement, "title") ?? "";

                // Link
                item.Link = GetElementValue(itemElement, "link") ?? "";

                // Description
                item.Summary = GetElementValue(itemElement, "description") ?? "";

                // Content:encoded (full content) - öncelikli olarak al
                var encodedContent = GetElementValue(itemElement, "content:encoded") ??
                                     GetElementValue(itemElement, "encoded") ??
                                     GetElementValueByLocalName(itemElement, "encoded");

                // Content'i belirle - önce encoded content, yoksa description
                if (!string.IsNullOrEmpty(encodedContent))
                {
                    item.Content = encodedContent;
                    // Eğer summary boşsa, content'in ilk kısmını al
                    if (string.IsNullOrEmpty(item.Summary))
                    {
                        item.Summary = TruncateHtml(encodedContent, 500);
                    }
                }
                else
                {
                    // Encoded content yoksa description'ı kullan
                    item.Content = item.Summary;
                }

                // PubDate - çeşitli formatları dene
                var pubDateStr = GetElementValue(itemElement, "pubDate");
                item.PublishDate = ParseDateTime(pubDateStr);

                // Author
                item.Author = GetElementValue(itemElement, "author") ??
                              GetElementValue(itemElement, "dc:creator") ?? "";

                // Categories
                item.Categories = itemElement.Elements("category")
                    .Select(e => e.Value)
                    .ToList();

                // Image - enclosure veya media:content
                item.ImageUrl = ExtractImageFromRssItem(itemElement);

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Atom formatını parse eder
        /// </summary>
        private List<RssFeedItem> ParseAtom(XDocument doc)
        {
            var items = new List<RssFeedItem>();

            // Atom namespace
            XNamespace atom = "http://www.w3.org/2005/Atom";
            var ns = doc.Root?.GetDefaultNamespace() ?? atom;

            var entries = doc.Descendants(ns + "entry");

            foreach (var entry in entries)
            {
                var item = new RssFeedItem();

                // Title
                item.Title = GetElementValue(entry, "title", ns) ?? "";

                // Link
                var linkElement = entry.Elements(ns + "link")
                    .FirstOrDefault(e => e.Attribute("rel")?.Value == "alternate" ||
                                        e.Attribute("rel") == null);
                item.Link = linkElement?.Attribute("href")?.Value ?? "";

                // Summary
                item.Summary = GetElementValue(entry, "summary", ns) ?? "";

                // Content
                var contentValue = GetElementValue(entry, "content", ns);
                item.Content = !string.IsNullOrEmpty(contentValue) ? contentValue : item.Summary;

                // Published/Updated
                var publishedStr = GetElementValue(entry, "published", ns) ??
                                   GetElementValue(entry, "updated", ns);
                item.PublishDate = ParseDateTime(publishedStr);

                // Author
                var authorElement = entry.Element(ns + "author");
                if (authorElement != null)
                {
                    item.Author = GetElementValue(authorElement, "name", ns) ?? "";
                }

                // Categories
                item.Categories = entry.Elements(ns + "category")
                    .Select(e => e.Attribute("term")?.Value ?? e.Value)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                // Image
                item.ImageUrl = ExtractImageFromAtomEntry(entry, ns);

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// RDF (RSS 1.0) formatını parse eder
        /// </summary>
        private List<RssFeedItem> ParseRdf(XDocument doc)
        {
            var items = new List<RssFeedItem>();

            // RDF namespace
            XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
            XNamespace rss = "http://purl.org/rss/1.0/";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";

            var itemElements = doc.Descendants(rss + "item");

            foreach (var itemElement in itemElements)
            {
                var item = new RssFeedItem();

                item.Title = GetElementValue(itemElement, "title", rss) ?? "";
                item.Link = GetElementValue(itemElement, "link", rss) ?? "";
                item.Summary = GetElementValue(itemElement, "description", rss) ?? "";
                item.Content = item.Summary;

                var dateStr = GetElementValue(itemElement, "date", dc);
                item.PublishDate = ParseDateTime(dateStr);

                item.Author = GetElementValue(itemElement, "creator", dc) ?? "";

                item.Categories = itemElement.Elements(dc + "subject")
                    .Select(e => e.Value)
                    .ToList();

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// XML element değerini güvenli şekilde alır
        /// </summary>
        private string GetElementValue(XElement parent, string elementName, XNamespace ns = null)
        {
            try
            {
                if (ns != null)
                {
                    return parent.Element(ns + elementName)?.Value?.Trim();
                }
                else
                {
                    // Namespace olmadan ara, varsa namespace ile de dene
                    var element = parent.Element(elementName);
                    if (element == null)
                    {
                        // Tüm namespace'lerde ara
                        element = parent.Elements()
                            .FirstOrDefault(e => e.Name.LocalName == elementName);
                    }
                    return element?.Value?.Trim();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// XML element'i local name'e göre bulur (namespace'den bağımsız)
        /// </summary>
        private string GetElementValueByLocalName(XElement parent, string localName)
        {
            try
            {
                var element = parent.Elements()
                    .FirstOrDefault(e => e.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase));
                return element?.Value?.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// HTML içeriği belirtilen karakter sayısında keser
        /// </summary>
        private string TruncateHtml(string html, int maxLength)
        {
            if (string.IsNullOrEmpty(html))
                return html;

            // HTML tag'lerini kaldır
            var plainText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
            // Birden fazla boşluğu tek boşluğa indir
            plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();

            if (plainText.Length <= maxLength)
                return plainText;

            return plainText.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// DateTime string'ini parse eder (çeşitli formatları destekler)
        /// </summary>
        private DateTime ParseDateTime(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return DateTime.Now;

            try
            {
                // RFC 822 (RSS 2.0), RFC 3339 (Atom), ISO 8601 formatlarını dene
                if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date1))
                    return date1;

                if (DateTimeOffset.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date2))
                    return date2.DateTime;

                // Türkçe formatları dene
                if (DateTime.TryParse(dateStr, new CultureInfo("tr-TR"), DateTimeStyles.None, out var date3))
                    return date3;

                // Unix timestamp dene
                if (long.TryParse(dateStr, out var timestamp))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
                }
            }
            catch
            {
                // Parse hatası
            }

            return DateTime.Now;
        }

        /// <summary>
        /// RSS item'dan resim URL'sini çıkarır
        /// </summary>
        private string ExtractImageFromRssItem(XElement itemElement)
        {
            try
            {
                // 1. Enclosure
                var enclosure = itemElement.Elements("enclosure")
                    .FirstOrDefault(e => e.Attribute("type")?.Value?.StartsWith("image/") ?? false);
                if (enclosure != null)
                {
                    var url = enclosure.Attribute("url")?.Value;
                    if (!string.IsNullOrEmpty(url))
                        return url;
                }

                // 2. Media:content veya media:thumbnail
                var mediaContent = itemElement.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "content" || e.Name.LocalName == "thumbnail");
                if (mediaContent != null)
                {
                    var url = mediaContent.Attribute("url")?.Value;
                    if (!string.IsNullOrEmpty(url))
                        return url;
                }

                // 3. Description veya content:encoded içinden img tag
                var description = GetElementValue(itemElement, "description") ??
                                  GetElementValue(itemElement, "content:encoded") ??
                                  GetElementValue(itemElement, "encoded");
                if (!string.IsNullOrEmpty(description))
                {
                    var imgUrl = ExtractImageFromHtml(description);
                    if (!string.IsNullOrEmpty(imgUrl))
                        return imgUrl;
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        /// <summary>
        /// Atom entry'den resim URL'sini çıkarır
        /// </summary>
        private string ExtractImageFromAtomEntry(XElement entry, XNamespace ns)
        {
            try
            {
                // 1. Link rel="enclosure"
                var enclosure = entry.Elements(ns + "link")
                    .FirstOrDefault(e => e.Attribute("rel")?.Value == "enclosure" &&
                                        (e.Attribute("type")?.Value?.StartsWith("image/") ?? false));
                if (enclosure != null)
                {
                    var url = enclosure.Attribute("href")?.Value;
                    if (!string.IsNullOrEmpty(url))
                        return url;
                }

                // 2. Content veya summary içinden img tag
                var content = GetElementValue(entry, "content", ns) ??
                              GetElementValue(entry, "summary", ns);
                if (!string.IsNullOrEmpty(content))
                {
                    var imgUrl = ExtractImageFromHtml(content);
                    if (!string.IsNullOrEmpty(imgUrl))
                        return imgUrl;
                }
            }
            catch
            {
                // Ignore
            }

            return null;
        }

        /// <summary>
        /// RSS feed'in geçerli olup olmadığını kontrol eder
        /// </summary>
        public async Task<bool> ValidateFeedUrlAsync(string feedUrl)
        {
            try
            {
                var response = await _httpClient.GetAsync(feedUrl);

                if (!response.IsSuccessStatusCode)
                    return false;

                var stream = await response.Content.ReadAsStreamAsync();

                // XML okuma ayarları - daha toleranslı
                var xmlReaderSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    CloseInput = false,
                    // Hatalı karakterleri görmezden gel
                    CheckCharacters = false
                };

                // İlk önce SyndicationFeed ile dene
                try
                {
                    stream.Position = 0;
                    using (var xmlReader = XmlReader.Create(stream, xmlReaderSettings))
                    {
                        var feed = SyndicationFeed.Load(xmlReader);
                        return feed != null;
                    }
                }
                catch (Exception ex) when (ex is XmlException || ex is FormatException || ex is InvalidOperationException)
                {
                    // SyndicationFeed.Load başarısız oldu, manuel parsing dene
                    try
                    {
                        stream.Position = 0;
                        var doc = XDocument.Load(stream);
                        var root = doc.Root;

                        if (root == null)
                            return false;

                        // RSS, Atom veya RDF formatını kontrol et
                        return root.Name.LocalName == "rss" ||
                               root.Name.LocalName == "feed" ||
                               root.Name.LocalName == "RDF";
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// RSS feed item'dan resim URL'sini çıkarır (enclosure, media:content, media:thumbnail, HTML parse)
        /// </summary>
        private string ExtractImageUrl(SyndicationItem feedItem, string content, string summary)
        {
            // 1. Önce enclosure tag'ini kontrol et (en yaygın RSS resim formatı)
            try
            {
                var enclosure = feedItem.Links?.FirstOrDefault(link =>
                    link.RelationshipType == "enclosure" &&
                    (link.MediaType?.StartsWith("image/") ?? false));

                if (enclosure != null && enclosure.Uri != null)
                {
                    return enclosure.Uri.ToString();
                }
            }
            catch
            {
                // Enclosure parse hatası - devam et
            }

            // 2. Media RSS extension (media:content, media:thumbnail, media:image)
            try
            {
                foreach (var ext in feedItem.ElementExtensions)
                {
                    // Çeşitli media namespace'leri kontrol et
                    if (ext.OuterName == "thumbnail" || ext.OuterName == "content" ||
                        ext.OuterName == "image" || ext.OuterName == "group")
                    {
                        using (var reader = ext.GetReader())
                        {
                            // url attribute'unu al
                            var url = reader.GetAttribute("url");
                            if (!string.IsNullOrEmpty(url))
                            {
                                return url;
                            }

                            // Eğer media:group ise, içindeki media:content veya media:thumbnail'i bul
                            if (ext.OuterName == "group")
                            {
                                while (reader.Read())
                                {
                                    if (reader.NodeType == XmlNodeType.Element &&
                                        (reader.LocalName == "content" || reader.LocalName == "thumbnail"))
                                    {
                                        url = reader.GetAttribute("url");
                                        if (!string.IsNullOrEmpty(url))
                                        {
                                            return url;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // description içindeki CDATA'da resim olabilir
                    else if (ext.OuterName == "encoded" || ext.OuterName == "description")
                    {
                        try
                        {
                            var encodedContent = ext.GetObject<string>();
                            if (!string.IsNullOrEmpty(encodedContent))
                            {
                                var imgUrl = ExtractImageFromHtml(encodedContent);
                                if (!string.IsNullOrEmpty(imgUrl))
                                {
                                    return imgUrl;
                                }
                            }
                        }
                        catch
                        {
                            // Extension parse hatası - devam et
                        }
                    }
                }
            }
            catch
            {
                // Media extension parse hatası - devam et
            }

            // 3. HTML içerikten img tag'i parse et
            var htmlContent = content ?? summary;
            if (!string.IsNullOrEmpty(htmlContent))
            {
                var imgUrl = ExtractImageFromHtml(htmlContent);
                if (!string.IsNullOrEmpty(imgUrl))
                {
                    return imgUrl;
                }
            }

            return null;
        }

        /// <summary>
        /// HTML içerikten img tag'inden resim URL'sini çıkarır
        /// </summary>
        private string ExtractImageFromHtml(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return null;

            try
            {
                // Önce src="..." veya src='...' pattern'ini dene
                var imgPattern = @"<img[^>]+src\s*=\s*[""']([^""']+)[""']";
                var match = System.Text.RegularExpressions.Regex.Match(htmlContent, imgPattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (match.Success && match.Groups.Count > 1)
                {
                    var url = match.Groups[1].Value;
                    // Relative URL'leri filtrele (data: ile başlayanları kabul et)
                    if (url.StartsWith("http") || url.StartsWith("//") || url.StartsWith("data:"))
                    {
                        // // ile başlayanları http: ekleyerek düzelt
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
                // HTML parse hatası - ignore
            }

            return null;
        }
    }
}
