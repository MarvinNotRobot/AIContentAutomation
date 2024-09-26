using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Common.Content.Interface;
using Common.Content.Model;
using System.Threading.Tasks;
using WordpressAIFunction.Interface;
using WordpressAIFunction.RedditAPI;

namespace WordpressAIFunction.Tests
{
    public class BlogManagerTests
    {
        private readonly Mock<IContentGenerator> _mockContentGenerator;
        private readonly Mock<IOpenAPIImageGenerator> _mockImageGenerator;
        private readonly Mock<IContentScraper> _mockContentScraper;
        private readonly Mock<IBlogPoster> _mockBlogPoster;
        private readonly Mock<ILogger<BlogManager>> _mockLogger;
        private readonly BlogManager _blogManager;

        public BlogManagerTests()
        {
            _mockContentGenerator = new Mock<IContentGenerator>();
            _mockImageGenerator = new Mock<IOpenAPIImageGenerator>();
            _mockContentScraper = new Mock<IContentScraper>();
            _mockBlogPoster = new Mock<IBlogPoster>();
            _mockLogger = new Mock<ILogger<BlogManager>>();

            _blogManager = new BlogManager(
                _mockContentGenerator.Object,
                _mockImageGenerator.Object,
                _mockContentScraper.Object,
                _mockBlogPoster.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task RunAsync_ScrapePostsReturnsEmptyString_DoesNotGenerateContent()
        {
            // Arrange
            _mockContentScraper.Setup(scraper => scraper.ScrapePostsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(string.Empty);

            // Act
            await _blogManager.RunAsync();

            // Assert
            _mockContentGenerator.Verify(generator => generator.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockBlogPoster.Verify(poster => poster.PostToWordPressAsync(It.IsAny<Article>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_GeneratedContentIsNull_DoesNotPostToWordPress()
        {
            // Arrange
            _mockContentScraper.Setup(scraper => scraper.ScrapePostsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("seed posts");
            _mockContentGenerator.Setup(generator => generator.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Article)null);

            // Act
            await _blogManager.RunAsync();

            // Assert
            _mockBlogPoster.Verify(poster => poster.PostToWordPressAsync(It.IsAny<Article>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_GeneratedContentIsEmpty_DoesNotPostToWordPress()
        {
            // Arrange
            _mockContentScraper.Setup(scraper => scraper.ScrapePostsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("seed posts");
            _mockContentGenerator.Setup(generator => generator.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Article { body = string.Empty });

            // Act
            await _blogManager.RunAsync();

            // Assert
            _mockBlogPoster.Verify(poster => poster.PostToWordPressAsync(It.IsAny<Article>()), Times.Never);
        }

        [Fact]
        public async Task RunAsync_ValidGeneratedContent_PostsToWordPress()
        {
            // Arrange
            var article = new Article
            {
                title = "Test Title",
                body = "Test Body",
                category = "Test Category",
                summary = "Test Summary",
                tags = new[] { "tag1", "tag2" }
            };

            _mockContentScraper.Setup(scraper => scraper.ScrapePostsAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("seed posts");
            _mockContentGenerator.Setup(generator => generator.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(article);
            _mockImageGenerator.Setup(generator => generator.GenerateImageAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync("http://example.com/image.jpg");

            // Act
            await _blogManager.RunAsync();

            // Assert
            _mockBlogPoster.Verify(poster => poster.PostToWordPressAsync(It.Is<Article>(a => a.title == article.title && a.body == article.body)), Times.Once);
        }
    }
}

