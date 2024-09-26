using Microsoft.Extensions.Logging;
using Common.ShopifyAPI.Interface;
using Common.Content.Interface;

namespace ShopifyFunction
{
    
    public class ContentGeneratorSettings
    {
        public string Prompt;
    }

    public class ShopifyCommenter
    {
        private readonly ILogger<ShopifyCommenter> _logger;
        private readonly IShopifyProductService _shopifyService;
        private readonly IContentGenerator _openAPIContentGenerator;
        private readonly ContentGeneratorSettings _contentSettings;

        public ShopifyCommenter (ILogger<ShopifyCommenter> logger, IShopifyProductService shopifyService, IContentGenerator openAPIContentGenerator, ContentGeneratorSettings contentSettings)
        {
            _logger = logger;
            _shopifyService = shopifyService;
            _openAPIContentGenerator = openAPIContentGenerator;
            _contentSettings = contentSettings;
        }

    }
}
