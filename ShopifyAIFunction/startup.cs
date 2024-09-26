using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Common.ShopifyAPI.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Common.ShopifyAPI.Components;
using Common.OpenAPI;
using Microsoft.Extensions.Logging;
using Common.Content.Interface;
using ShopifyAIFunction;

[assembly: FunctionsStartup(typeof(ShopifyAI.Startup))]
namespace ShopifyAI
{
    public class Startup : FunctionsStartup
    {
        private readonly string ConfigKey_shopifyToken = "SHOPTOKEN";
        private readonly string ConfigKey_shopifyURL = "SHOP_URL";

        private readonly string ConfigKey_OpenaiKEY = "GENKEY";
        private readonly string ConfigKey_OpenaiAPP = "GEN_APP";


        private readonly string ConfigKey_GenAIPromptJsonFormat = "ChatGPT_CommandPromptWithJsonFormat";
        private readonly string ConfigKey_GenAIPromptPlaintTextFormat = "ChatGPT_CommandPromptWithPlainTextFormat";


        public IConfiguration _config;

        public Startup()
        {
        }


        public Startup(IConfiguration configuration)
        {
            _config = configuration;

        }

        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services.AddScoped<IShopifyProductService>((services) => 
                {
                    var config = services.GetService<IConfiguration>();
                    var _shopifyToken = config[ConfigKey_shopifyToken];
                    var _shopifyURL = config[ConfigKey_shopifyURL];
                    var obj = new ShopifyProductService(_shopifyURL, _shopifyToken);
                    return obj;
                }
            );

            builder.Services.AddScoped<IContentGenerator>((services) =>
            {
                var logger = services.GetService<ILogger>();
                var config = services.GetService<IConfiguration>();
                var _GENKEY = config[ConfigKey_OpenaiKEY];
                var _openAI_ORG = config[ConfigKey_OpenaiAPP];

                var obj = new OpenAPIContentGenerator(_GENKEY, _openAI_ORG,logger);
                return obj;
            });

             builder.Services.AddScoped<IOpenAPIImageGenerator>((services) =>
             {
                 var ImageGenerator = services.GetService<OpenAPIContentGenerator>();
                 return ImageGenerator;
             });

            builder.Services.AddScoped<IProcessHandler<int, string>, ShopifyContentProcessHandler>((services) =>
            {
                var logger = services.GetService<ILogger<ShopifyContentProcessHandler>>();
                var config = services.GetService<IConfiguration>();
                var _jsonFormatPrompt = config[ConfigKey_GenAIPromptJsonFormat];
                var _PlainFormatPrompt = config[ConfigKey_GenAIPromptPlaintTextFormat];
                var shopifyProductService = services.GetService<IShopifyProductService>();  
                var contentGenerator = services.GetService<IContentGenerator>();

                var obj = new ShopifyContentProcessHandler(logger, shopifyProductService, contentGenerator, _jsonFormatPrompt, _PlainFormatPrompt);
                return obj;
            });


        }
    }
   
}
