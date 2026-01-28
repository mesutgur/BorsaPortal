using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorsaPortal.Application.Services;
using BorsaPortal.Domain.Entities;
using BorsaPortal.Infrastructure.Data;

namespace BorsaPortal.Infrastructure.Services
{
    /// <summary>
    /// RSS feed'lerden haber senkronizasyonu yapan servis
    /// </summary>
    public class NewsSyncService
    {
        private readonly BorsaPortalDbContext _context;
        private readonly IRssFeedService _rssFeedService;

        public NewsSyncService(BorsaPortalDbContext context, IRssFeedService rssFeedService)
        {
            _context = context;
            _rssFeedService = rssFeedService;
        }

        /// <summary>
        /// Tüm aktif RSS feed kaynaklarından haberleri senkronize eder
        /// </summary>
        public async Task<NewsSyncResult> SyncAllFeedsAsync()
        {
            var result = new NewsSyncResult();

            try
            {
                var activeSources = _context.RssFeedSources
                    .Where(r => r.IsActive && !r.IsDeleted)
                    .ToList();

                if (!activeSources.Any())
                {
                    result.Success = false;
                    result.Message = "Aktif RSS kaynağı bulunamadı. Lütfen önce RSS kaynağı ekleyin ve aktif hale getirin.";
                    return result;
                }

                int successCount = 0;
                int failCount = 0;

                foreach (var source in activeSources)
                {
                    var sourceResult = await SyncFeedAsync(source);
                    result.AddedCount += sourceResult.AddedCount;
                    result.SkippedCount += sourceResult.SkippedCount;
                    result.SourceResults.Add(sourceResult);

                    if (sourceResult.Success)
                        successCount++;
                    else
                        failCount++;
                }

                result.Success = true;

                // Detaylı mesaj oluştur
                var messageParts = new List<string>();

                if (result.AddedCount > 0)
                {
                    messageParts.Add($"✓ Toplam {result.AddedCount} yeni haber eklendi");
                }

                if (result.SkippedCount > 0)
                {
                    messageParts.Add($"{result.SkippedCount} haber zaten mevcut");
                }

                if (failCount > 0)
                {
                    messageParts.Add($"⚠ {failCount} kaynakta hata oluştu");
                }

                messageParts.Add($"{successCount}/{activeSources.Count} kaynak başarılı");

                result.Message = string.Join(" • ", messageParts);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Kritik hata: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Belirli bir RSS feed kaynağından haberleri senkronize eder
        /// </summary>
        public async Task<FeedSyncResult> SyncFeedAsync(RssFeedSource source)
        {
            var result = new FeedSyncResult
            {
                SourceName = source.Name,
                SourceId = source.Id
            };

            try
            {
                // RSS feed'den haberleri çek
                List<RssFeedItem> feedItems;

                try
                {
                    feedItems = await _rssFeedService.FetchFeedItemsAsync(source);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Message = $"RSS feed okunamadı: {ex.Message}";
                    return result;
                }

                if (feedItems == null || !feedItems.Any())
                {
                    result.Success = false;
                    result.Message = "RSS feed'den haber bulunamadı. Feed boş veya geçersiz olabilir.";
                    return result;
                }

                foreach (var item in feedItems)
                {
                    // Duplicate kontrolü - SourceUrl'e göre
                    if (!string.IsNullOrEmpty(item.Link))
                    {
                        var exists = _context.News
                            .Any(n => n.SourceUrl == item.Link && !n.IsDeleted);

                        if (exists)
                        {
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    // Title duplicate kontrolü
                    var titleExists = _context.News
                        .Any(n => n.Title == item.Title && !n.IsDeleted);

                    if (titleExists)
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    // News entity oluştur
                    var news = new News
                    {
                        Title = TruncateString(item.Title, 500),
                        Content = item.Content,
                        Summary = !string.IsNullOrEmpty(item.Summary)
                            ? TruncateString(item.Summary, 1000)
                            : TruncateString(item.Content, 500),
                        PublishDate = item.PublishDate,
                        Source = source.Name,
                        SourceUrl = TruncateString(item.Link, 500),
                        ImageUrl = TruncateString(item.ImageUrl, 500),
                        IsAutoFetched = true,
                        IsFeatured = false,
                        SectorId = source.SectorId,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    // Kategorilerden şirket veya sektör eşleştirmesi yapılabilir
                    TryMatchCompanyOrSector(news, item);

                    _context.News.Add(news);
                    result.AddedCount++;
                }

                // Feed source güncelle
                source.LastFetchedAt = DateTime.Now;
                source.ItemsFetchedCount += result.AddedCount;

                _context.SaveChanges();

                result.Success = true;

                if (result.AddedCount > 0)
                {
                    result.Message = $"✓ {result.AddedCount} yeni haber eklendi" +
                                   (result.SkippedCount > 0 ? $", {result.SkippedCount} haber zaten mevcut (atlandı)" : "");
                }
                else if (result.SkippedCount > 0)
                {
                    result.Message = $"Yeni haber yok. {result.SkippedCount} haber zaten sistemde mevcut.";
                }
                else
                {
                    result.Message = "Yeni haber bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Veritabanı hatası: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Belirli bir RSS feed kaynağını ID'ye göre senkronize eder
        /// </summary>
        public async Task<FeedSyncResult> SyncFeedByIdAsync(int sourceId)
        {
            var source = _context.RssFeedSources
                .FirstOrDefault(r => r.Id == sourceId && !r.IsDeleted);

            if (source == null)
            {
                return new FeedSyncResult
                {
                    Success = false,
                    Message = "RSS kaynağı bulunamadı.",
                    SourceId = sourceId
                };
            }

            return await SyncFeedAsync(source);
        }

        /// <summary>
        /// Kategoriler ve içerikten şirket/sektör eşleştirmesi yapar
        /// </summary>
        private void TryMatchCompanyOrSector(News news, RssFeedItem item)
        {
            // Şirket eşleştirme
            var companies = _context.Companies.Where(c => !c.IsDeleted).ToList();
            var searchText = $"{item.Title} {item.Summary} {item.Content}".ToUpper();

            foreach (var company in companies)
            {
                if (!string.IsNullOrEmpty(company.Code) &&
                    searchText.Contains(company.Code.ToUpper()))
                {
                    news.CompanyId = company.Id;
                    if (company.SectorId.HasValue && !news.SectorId.HasValue)
                    {
                        news.SectorId = company.SectorId;
                    }
                    break;
                }

                if (!string.IsNullOrEmpty(company.Name) &&
                    searchText.Contains(company.Name.ToUpper()))
                {
                    news.CompanyId = company.Id;
                    if (company.SectorId.HasValue && !news.SectorId.HasValue)
                    {
                        news.SectorId = company.SectorId;
                    }
                    break;
                }
            }

            // Sektör eşleştirme (eğer şirket eşleşmemişse)
            if (!news.SectorId.HasValue && !news.CompanyId.HasValue)
            {
                var sectors = _context.Sectors.Where(s => !s.IsDeleted).ToList();

                foreach (var sector in sectors)
                {
                    if (!string.IsNullOrEmpty(sector.Name) &&
                        searchText.Contains(sector.Name.ToUpper()))
                    {
                        news.SectorId = sector.Id;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// String'i belirtilen uzunlukta keser
        /// </summary>
        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    /// <summary>
    /// Haber senkronizasyon sonuç bilgisi
    /// </summary>
    public class NewsSyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int AddedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<FeedSyncResult> SourceResults { get; set; }

        public NewsSyncResult()
        {
            SourceResults = new List<FeedSyncResult>();
        }
    }

    /// <summary>
    /// Feed kaynağı senkronizasyon sonucu
    /// </summary>
    public class FeedSyncResult
    {
        public int SourceId { get; set; }
        public string SourceName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int AddedCount { get; set; }
        public int SkippedCount { get; set; }
    }
}
