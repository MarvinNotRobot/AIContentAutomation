using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.ShopifyAPI.Interface;
using Common.Content.Interface;
using Common.Content.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ShopifySharp;
using TestHelper;
using System.Reflection;
using Common.OpenAPI;
using static Azure.Core.HttpHeader;

namespace ShopifyAIFunction.Tests
{
    public class ShopifyContentProcessHandlerTests
    {
        private readonly TestLogger<ShopifyContentProcessHandler> _logger;

        private readonly Mock<IShopifyProductService> _shopifyServiceMock;
        private readonly Mock<IContentGenerator> _contentGeneratorMock;
        private readonly ShopifyContentProcessHandler _handler;
        

        public ShopifyContentProcessHandlerTests()
        {
            _logger = new TestLogger<ShopifyContentProcessHandler>();
            _shopifyServiceMock = new Mock<IShopifyProductService>();
            _contentGeneratorMock = new Mock<IContentGenerator>();
            _handler = new ShopifyContentProcessHandler(
                _logger,
                _shopifyServiceMock.Object,
                _contentGeneratorMock.Object,
                "jsonFormat",
                "plainTextFormat"
            );
        }

        [Fact]
        public async Task ProcessRequest_ShouldReturnOk_WhenProductsProcessedSuccessfully()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, BodyHtml = "<p>Test</p>", Title = "Test Product" }
            };
            _shopifyServiceMock.Setup(s => s.ListProducts(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(products);
            _contentGeneratorMock.Setup(c => c.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Common.Content.Model.Article { body = "Rewritten Content", tags = new string[] { "tag1" } });

            // Act
            var result = await _handler.ProcessRequest(1);

            // Assert
            Assert.Equal("OK", result);
            _shopifyServiceMock.Verify(s => s.UpdateProduct(1, "Rewritten Content", "Test Product", null, new string[] { "tag1" }), Times.Once);
        }

        [Fact]
        // fix this test
        public async Task ProcessRequest_ShouldLogError_WhenRewriteFails()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, BodyHtml = "<p>Test</p>", Title = "Test Product" }
            };
            _shopifyServiceMock.Setup(s => s.ListProducts(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(products);
            _contentGeneratorMock.Setup(c => c.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Common.Content.Model.Article)null);

            // Act
            var result = await _handler.ProcessRequest(1);

            // Assert
            Assert.Equal("OK", result);
            var logEntry = _logger.Logs.FirstOrDefault(log => log.LogLevel == LogLevel.Error);
            Assert.NotNull(logEntry);
            Assert.Contains("Rewrite failed", logEntry.Message);
        }

        [Fact]
        public async Task ProcessRequest_ShouldLogException_WhenExceptionOccurs()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, BodyHtml = "<p>Test</p>", Title = "Test Product" }
            };
            _shopifyServiceMock.Setup(s => s.ListProducts(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(products);
            _contentGeneratorMock.Setup(c => c.GenerateBlogPost(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test Exception"));

            // Act
            var result = await _handler.ProcessRequest(1);

            // Assert
            Assert.Equal("OK", result);
            var logEntry = _logger.Logs.FirstOrDefault(log => log.LogLevel == LogLevel.Error);
            Assert.NotNull(logEntry);
        }

        [Fact]
        public void RemoveHtmlTags_ShouldRemoveHtmlTagsAndEmojis()
        {
            // Arrange
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("RemoveHtmlTags", BindingFlags.NonPublic | BindingFlags.Instance);
            var input = "<p>Hello, world!</p>";
            var expectedOutput = "Hello, world!";

            // Act
            var result = methodInfo.Invoke(_handler, new object[] { input }) as string;

            // Assert
            Assert.Equal(expectedOutput, result.Trim());
        }

        [Fact]
        public void RemoveEmojis_ShouldRemoveEmojis()
        {
            // Arrange
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("RemoveEmojis", BindingFlags.NonPublic | BindingFlags.Static);
            var input = "Hello, world! 🌍";
            var expectedOutput = "Hello, world!";

            // Act
            var result = methodInfo.Invoke(null, new object[] { input }) as string;

            // Assert
            Assert.Equal(expectedOutput, result.Trim());
        }

        [Fact]
        public async Task GetRewrittenArticle_ShouldReturnArticle_WhenContentIsGenerated()
        {
            // Arrange
            var productBodyHTML = "Sample product body HTML";
            var expectedArticle = new Common.Content.Model.Article { body = "Rewritten Content", tags = new string[] { "tag1" } };
            _contentGeneratorMock.Setup(c => c.GenerateBlogPost(productBodyHTML, "jsonFormat"))
                .ReturnsAsync(expectedArticle);

            // Act
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("GetRewrittenArticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<Common.Content.Model.Article>)methodInfo.Invoke(_handler, new object[] { productBodyHTML });
            var result = await task;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedArticle.body, result.body);
            Assert.Equal(expectedArticle.tags, result.tags);
        }


        [Fact]
        public async Task GetRewrittenArticle_ShouldReturnNull_WhenContentGenerationFails()
        {
            // Arrange
            var productBodyHTML = "Sample product body HTML";
            _contentGeneratorMock.Setup(c => c.GenerateBlogPost(productBodyHTML, "jsonFormat"))
                .ThrowsAsync(new OpenAIPromptTooLongException("OpenAIPromptTooLongException"));

            // Act
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("GetRewrittenArticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<Common.Content.Model.Article>)methodInfo.Invoke(_handler, new object[] { productBodyHTML });
            var result = await task;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SummarizeArticle_ShouldReturnSummarizedArticle_WhenContentIsSummarized()
        {
            // Arrange
            var productBodyHTML = "Sample product body HTML";
            var size = 100;
            var summarizedContent = "Summarized Content";
            _contentGeneratorMock.Setup(c => c.GenerateBlogPostInPlainText(productBodyHTML, It.IsAny<string>()))
                .ReturnsAsync(summarizedContent);

            // Act
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("SummarizeArticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<Common.Content.Model.Article>)methodInfo.Invoke(_handler, new object[] { productBodyHTML, size });
            var result = await task;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(summarizedContent, result.body);
        }


        [Fact]
        public async Task SummarizeArticle_ShouldReturnNull_WhenContentSummarizationFails()
        {
            // Arrange
            var productBodyHTML = "Sample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTMLSample product body HTML";
            var size = 100;
            _contentGeneratorMock.Setup(c => c.GenerateBlogPostInPlainText(productBodyHTML, It.IsAny<string>()))
                .ThrowsAsync(new OpenAIPromptTooLongException("OpenAIPromptTooLongException"));

            // Act
            var methodInfo = typeof(ShopifyContentProcessHandler).GetMethod("SummarizeArticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<Common.Content.Model.Article>)methodInfo.Invoke(_handler, new object[] { productBodyHTML, size });
            var result = await task;

            // Assert
            Assert.Null(result);
        }
    }
}
