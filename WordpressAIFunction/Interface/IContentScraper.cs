
using System.Threading.Tasks;

namespace WordpressAIFunction.Interface
{
    public interface IContentScraper
    {
        Task<string> ScrapePostsAsync(string url, int numPosts);
    }
}
