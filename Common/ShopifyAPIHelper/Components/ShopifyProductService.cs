using Common.ShopifyAPI.Interface;
using ShopifySharp;
using ShopifySharp.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.ShopifyAPI.Components
{
    /// <summary>
    /// Service for managing Shopify products.
    /// </summary>
    public class ShopifyProductService : IShopifyProductService
    {
        private readonly string _shopifyUrl;
        private readonly string _accessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopifyProductService"/> class.
        /// </summary>
        /// <param name="shopifyUrl">The Shopify store URL.</param>
        /// <param name="accessToken">The access token for the Shopify store.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="shopifyUrl"/> or <paramref name="accessToken"/> is null.</exception>
        public ShopifyProductService(string shopifyUrl, string accessToken)
        {
            _ = shopifyUrl ?? throw new ArgumentNullException($"ShopifyProductService() constructor: {nameof(shopifyUrl)}");
            _ = accessToken ?? throw new ArgumentNullException($"ShopifyProductService() constructor: {nameof(accessToken)}");

            _shopifyUrl = shopifyUrl;
            _accessToken = accessToken;

            // Ensure _shopifyUrl starts with https://
            if (!_shopifyUrl.StartsWith("https://"))
            {
                _shopifyUrl = $"https://{_shopifyUrl}";
            }
        }

        /// <summary>
        /// Creates a new product in the Shopify store.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The ID of the created product.</returns>
        public async Task<long> CreateProduct(Product product)
        {
            var service = new ProductService(_shopifyUrl, _accessToken);
            return (await service.CreateAsync(product)).Id.Value;
        }

        /// <summary>
        /// Lists products created within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the date range.</param>
        /// <param name="endDate">The end date of the date range.</param>
        /// <returns>A collection of products created within the specified date range.</returns>
        public async Task<IEnumerable<Product>> ListProducts(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var service = new ProductService(_shopifyUrl, _accessToken);
            var filter = new ProductListFilter
            {
                CreatedAtMin = startDate,
                CreatedAtMax = endDate,
            };

            var returnResult = await service.ListAsync(filter);
            return (IEnumerable<Product>)returnResult.Items;
        }

        /// <summary>
        /// Updates an existing product in the Shopify store.
        /// </summary>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="bodyHtml">The new HTML body content of the product.</param>
        /// <param name="title">The new title of the product.</param>
        /// <param name="metaData">The new metadata for the product.</param>
        /// <param name="tags">The new tags for the product.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <exception cref="Exception">Thrown when the product with the specified ID is not found.</exception>
        public async Task UpdateProduct(long productId, string bodyHtml = null, string title = null, IDictionary<string, string> metaData = null, string[] tags = null)
        {
            var service = new ProductService(_shopifyUrl, _accessToken);
            var product = await service.GetAsync(productId);

            if (product == null)
            {
                throw new Exception($"Product with ID {productId} not found.");
            }

            if (bodyHtml != null)
            {
                product.BodyHtml = bodyHtml;
            }

            if (title != null)
            {
                product.Title = title;
            }

            if (tags != null && tags.Length > 0)
            {
                product.Tags = string.Join(",", tags);
            }

            if (metaData != null)
            {
                foreach (var kvp in metaData)
                {
                    product.Metafields.Append(new MetaField
                    {
                        Namespace = kvp.Key,
                        Key = kvp.Key,
                        Value = kvp.Value
                    });
                }
            }

            await service.UpdateAsync(productId, product);
        }
    }
}
