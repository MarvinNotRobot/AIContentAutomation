using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WordpressAIFunction.Interface;

namespace WordpressAIFunction.RedditAPI
{
    /// <summary>
    /// Class responsible for scraping posts from a specified Reddit subreddit.
    /// </summary>
    public class RedditPostScraper : IContentScraper
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedditPostScraper"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client instance.</param>
        public RedditPostScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Scrapes posts from a specified subreddit URL.
        /// </summary>
        /// <param name="subredditUrl">The URL of the subreddit.</param>
        /// <param name="numPosts">The number of posts to scrape.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the title of a randomly selected post.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while scraping posts.</exception>
        public async Task<string> ScrapePostsAsync(string subredditUrl, int numPosts)
        {
            if (numPosts <= 2)
                numPosts = 10;
            try
            {
                using HttpClient httpClient = _httpClient;

                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:106.0) Gecko/20100101 Firefox/106.0");
                httpClient.DefaultRequestHeaders.Add("X-Forwarded-For", "1.2.3.4");

                string jsonUrl = $"{subredditUrl}.json?limit={numPosts}";
                string jsonResponse = await httpClient.GetStringAsync(jsonUrl);
                JsonDocument jsonDocument = JsonDocument.Parse(jsonResponse);
                var posts = jsonDocument.RootElement.GetProperty("data")
                    .GetProperty("children")
                    .EnumerateArray()
                    .Select(post => post.GetProperty("data").GetProperty("title").GetString())
                    .Where(title => title.Length > 20)
                    .ToArray();

                if (posts.Length == 0)
                {
                    return null;
                }

                var random = new Random();
                int selectedIndex = random.Next(posts.Length);
                return posts[selectedIndex];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting the scrapper:  {ex.Message}");
                throw;
            }
        }
    }
}