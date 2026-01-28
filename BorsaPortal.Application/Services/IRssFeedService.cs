using System.Collections.Generic;
using System.Threading.Tasks;
using BorsaPortal.Domain.Entities;

namespace BorsaPortal.Application.Services
{
    /// <summary>
    /// RSS feed okuma ve parse etme servisi
    /// </summary>
    public interface IRssFeedService
    {
        /// <summary>
        /// RSS feed URL'inden haberleri çeker ve parse eder
        /// </summary>
        Task<List<RssFeedItem>> FetchFeedItemsAsync(string feedUrl);

        /// <summary>
        /// RSS feed source'undan haberleri çeker
        /// </summary>
        Task<List<RssFeedItem>> FetchFeedItemsAsync(RssFeedSource feedSource);

        /// <summary>
        /// RSS feed'in geçerli olup olmadığını kontrol eder
        /// </summary>
        Task<bool> ValidateFeedUrlAsync(string feedUrl);
    }

    /// <summary>
    /// RSS feed item - parse edilen haber verisi
    /// </summary>
    public class RssFeedItem
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public string ImageUrl { get; set; }
        public System.DateTime PublishDate { get; set; }
        public string Author { get; set; }
        public List<string> Categories { get; set; }

        public RssFeedItem()
        {
            Categories = new List<string>();
        }
    }
}
