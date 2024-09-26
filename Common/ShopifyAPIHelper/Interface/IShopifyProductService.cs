using ShopifySharp;

namespace Common.ShopifyAPI.Interface
{
    /// <summary>
    /// Interface for managing Shopify products.
    /// </summary>
    public interface IShopifyProductService
    {
        /// <summary>
        /// Creates a new product in the Shopify store.
        /// </summary>
        /// <param name="product">The product to create.</param>
        /// <returns>The ID of the created product.</returns>
        Task<long> CreateProduct(Product product);

        /// <summary>
        /// Lists products created within a specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the date range.</param>
        /// <param name="endDate">The end date of the date range.</param>
        /// <returns>A collection of products created within the specified date range.</returns>
        Task<IEnumerable<Product>> ListProducts(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Updates an existing product in the Shopify store.
        /// </summary>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="bodyHtml">The new HTML body content of the product.</param>
        /// <param name="title">The new title of the product.</param>
        /// <param name="metaData">The new metadata for the product.</param>
        /// <param name="tags">The new tags for the product.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateProduct(long productId, string bodyHtml = null, string title = null, IDictionary<string, string> metaData = null, string[] tags = null);
    }
}

