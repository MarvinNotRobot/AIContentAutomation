using System;

namespace Common.ShopifyAPI.Model
{
    /// <summary>
    /// Represents the settings required to connect to a Shopify store.
    /// </summary>
    public class ShopifySettings
    {
        /// <summary>
        /// Gets or sets the Shopify store URL.
        /// </summary>
        public string ShopifyURL { get; set; }

        /// <summary>
        /// Gets or sets the Shopify API key.
        /// </summary>
        public string ShopifyApiKey { get; set; }

        /// <summary>
        /// Gets or sets the Shopify API secret.
        /// </summary>
        public string ShopifyApiSecret { get; set; }

        /// <summary>
        /// Gets or sets the Shopify API token.
        /// </summary>
        public string ShopifyApiToken { get; set; }
    }
}

