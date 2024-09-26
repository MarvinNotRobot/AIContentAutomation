using System;
using System.Reflection;
using System.Threading.Tasks;
using Common.ShopifyAPI;
using Moq;
using ShopifySharp;
using Xunit;
using Common.ShopifyAPI.Components;


namespace Common.ShopifyAPI.UnitTests
{
    public class ShopifyProductServiceTests
    {
        private readonly ShopifyProductService _service;
        private readonly Mock<ProductService> _mockProductService;

        public ShopifyProductServiceTests()
        {
            var shopifyUrl = "https://example.myshopify.com";
            var accessToken = "test_access_token";
            _mockProductService = new Mock<ProductService>(shopifyUrl, accessToken);
            _service = new ShopifyProductService(shopifyUrl, accessToken);
        }

        // add unit test for ShopifyProductService constructor
        [Fact]
        public void ShopifyProductService_Constructor_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ShopifyProductService(null, "test_access_token"));
            Assert.Throws<ArgumentNullException>(() => new ShopifyProductService("https://example.myshopify.com", null));
        }

        [Fact]
        public void ShopifyProductService_Constructor_ChecksIfShopifyUrlStartsWithHttps()
        {
            var shopifyUrl = "example.myshopify.com";
            var service = new ShopifyProductService(shopifyUrl, "test_access_token");

            // Use reflection to access the private _shopifyUrl field
            var fieldInfo = typeof(ShopifyProductService).GetField("_shopifyUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var actualShopifyUrl = fieldInfo.GetValue(service) as string;

            // Check if _shopifyUrl starts with https://
            Assert.StartsWith("https://", actualShopifyUrl);
        }
    }
}