using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Common.OpenAPI;
using Microsoft.Extensions.Logging;
using Common.AWS.Model;
using Common.AWS;
using Common.Content.Interface;
using WordpressAIFunction.RedditAPI;
using WordpressAIFunction.Interface;
using WordpressAIFunction.Model;
using WordpressAIFunction.Wordpress;


[assembly: FunctionsStartup(typeof(WordpressAIFunction.Startup))]
namespace WordpressAIFunction
{
    public class Startup : FunctionsStartup
    {
        private const string ConfigKey_s3SettingsAccessKey = "s3SettingsAccessKey";
        private const string ConfigKey_s3SettingsSecretKey = "s3SettingsSecretKey";
        private const string ConfigKey_s3SettingsRegion = "s3SettingsRegion";
        private const string ConfigKey_s3ImageUploadPath = "s3ImageUploadPath";
        private const string s3imageBucketName = "s3imageBucketName";


        private readonly string ConfigKey_OpenaiKEY = "GENKEY";
        private readonly string ConfigKey_OpenaiAPP = "GEN_APP";

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
            builder.Services.AddLogging();
            builder.Services.AddHttpClient();


            // starting of image uploader
            builder.Services.AddSingleton<S3Settings>((services) =>
                    {
                        var configuration = services.GetService<IConfiguration>();


                        var s3AccessKey = configuration[ConfigKey_s3SettingsAccessKey];
                        var s3SecretKey = configuration[ConfigKey_s3SettingsSecretKey];
                        var s3Region = configuration[ConfigKey_s3SettingsRegion];
                        var s3ImageUploadPath = configuration[ConfigKey_s3ImageUploadPath];
                        var s3imageBucket = configuration[s3imageBucketName];

                        var s3settingObj = new S3Settings()
                        {
                            AccessKey = s3AccessKey,
                            Region = s3Region,
                            SecretKey = s3SecretKey,
                            imageUploadPath = s3ImageUploadPath,
                            imageBucket = s3imageBucket
                        };
                        return s3settingObj;
                    }
            );

            builder.Services.AddSingleton<IBlogPoster>( (services) => {
                
                var config = services.GetService<IConfiguration>();
                string wordpressApiUrl = config["WP_URL"];
                string username = config["WP_USERID"];
                string password = config["WP_PWD"];
                var imageUploader = services.GetService<IImageUploader>();
                var logger = services.GetService<ILogger<WordPressPoster>>();
                var wordpressConfig = new WordPressClientConfig()
                {
                    ApiUrl = wordpressApiUrl,
                    Username = username,
                    Password = password
                };
                var objWordpressPoster =new WordPressPoster(wordpressConfig, imageUploader, logger);
                return objWordpressPoster;
            }
            );

            builder.Services.AddHttpClient<IImageUploader, AwsImageUploader>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
            // end of image uploader

            builder.Services.AddScoped<IContentScraper, RedditPostScraper>();
            builder.Services.AddScoped<BlogManager>();

            builder.Services.AddScoped<OpenAPIContentGenerator>((services) =>
            {
                var logger = services.GetService<ILogger>();
                var config = services.GetService<IConfiguration>();
                var _GENKEY = "";//read from config[ConfigKey_OpenaiKEY];
                var _openAI_ORG = ""; //read from config[ConfigKey_OpenaiAPP];

                var obj = new OpenAPIContentGenerator(_GENKEY, _openAI_ORG, logger);
                return obj;
            });

     

            builder.Services.AddScoped<IContentGenerator>((services) =>
            {
                    var obj = services.GetService<OpenAPIContentGenerator>();
                    return obj;
            });

             builder.Services.AddScoped<IOpenAPIImageGenerator>((services) =>
             {
                 var obj = services.GetService<OpenAPIContentGenerator>();
                 return obj;
             }
           );

        }
    }
   
}
