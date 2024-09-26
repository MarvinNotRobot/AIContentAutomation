using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Content.Model;
using System;
using System.Threading.Tasks;
using Common.Content.Interface;
using WordpressAIFunction.RedditAPI;
using WordpressAIFunction.Interface;

namespace WordpressAIFunction
{
    public class BlogManager
    {
        private readonly IContentScraper _postScraper;
        private readonly IBlogPoster _wordPressPoster;
        private readonly IContentGenerator _openAPIContentGenerator;
        private readonly IOpenAPIImageGenerator _openAPIimageGenerator;
        private ILogger<BlogManager> _logger;
        public BlogManager(IContentGenerator openAPIContentGenerator, IOpenAPIImageGenerator openAPIimageGenerator, IContentScraper contentScraper, IBlogPoster wordPressPoster, ILogger<BlogManager> logger)
        {

            _wordPressPoster = wordPressPoster;
            _openAPIContentGenerator = openAPIContentGenerator;
            _openAPIimageGenerator = openAPIimageGenerator;
            _postScraper = contentScraper;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            // Scrape Reddit posts
            var seedPosts = await _postScraper.ScrapePostsAsync("https://www.reddit.com/r/beauty/new/", 30);
            if (string.IsNullOrWhiteSpace(seedPosts))
            {
                return;
            }

            // Generate content
            var generatedContent = await GetContent(seedPosts);

            if( string.IsNullOrWhiteSpace(generatedContent?.body ?? null))
            {
                _logger.LogInformation("No content returned from _contentGenerator");
                return;
            }    
            // Post content to WordPress blog
            await _wordPressPoster.PostToWordPressAsync(generatedContent);
        }

        private async Task<Article> GetContent(string seedText)
        {
            try
            {
                // Set up the prompt for the OpenAI API request
                var promptText = "Generate an SEO-optimized beauty, health and wellbeing blog post targeting middle age group women. The article should include a title, body, category, summary, and tags. body should be minimum 450 words. body can have emojis. body can have easy to read format with simple html tag such as 'p', 'em', 'h3', 'strong','span'. The category should be specific and relevant to the blog post. summary should highlight ingredients.tags can be maximum of 5 count. Make the article sound excited, happy, professional, funny, and friendly. Respond in JSON machine-readable format with the following keys: title, body, category, summary, and tags.";
                var article = await _openAPIContentGenerator.GenerateBlogPost(seedText, promptText);

                if (!string.IsNullOrEmpty(article?.body ?? null))
                {   
                    var imagePrompt = $"Create a high-quality blog post image with image size not exceeding 500 KB. description:'{article?.summary}'.";
                    article.imageUrl = await _openAPIimageGenerator.GenerateImageAsync(imagePrompt, 1);
                }
                return article;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetContent(). Un handled exception");
                 
                return null;
            }
        }
    }

}
