using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Content.Interface;
using Common.AWS.Model;

namespace Common.AWS.Extensions
{
    public static class AWSHelperExtensions
    {
        private const string ConfigKey_s3SettingsAccessKey  = "s3SettingsAccessKey";
        private const string ConfigKey_s3SettingsSecretKey = "s3SettingsSecretKey";
        private const string ConfigKey_s3SettingsRegion = "s3SettingsRegion";
        private const string ConfigKey_s3ImageUploadPath = "s3ImageUploadPath";
        private const string s3imageBucketName = "s3imageBucketName";

        public static IServiceCollection AddImageUploaderServices(this IServiceCollection services, IConfiguration configuration)
        {


            var s3AccessKey = configuration[ConfigKey_s3SettingsAccessKey];
            var s3SecretKey = configuration[ConfigKey_s3SettingsSecretKey];
            var s3Region = configuration[ConfigKey_s3SettingsRegion];
            var s3ImageUploadPath = configuration[ConfigKey_s3ImageUploadPath];
            var s3imageBucket = configuration[s3imageBucketName];

           

            if (string.IsNullOrWhiteSpace(s3AccessKey) || string.IsNullOrWhiteSpace(s3SecretKey) || string.IsNullOrWhiteSpace(s3Region))
            {
                var argumentException = new ArgumentNullException("Configuration might be missing following settings. {ConfigKey_s3SettingsAccessKey}, {ConfigKey_s3SettingsSecretKey}, {ConfigKey_s3SettingsRegion},{ConfigKey_s3ImageUploadPath}");
                throw new TypeInitializationException(typeof(S3Settings).FullName, argumentException);
            }
            var s3settingObj = new S3Settings()
            {
                AccessKey = s3AccessKey,
                Region = s3Region,
                SecretKey = s3SecretKey,
                imageUploadPath = s3ImageUploadPath,
                imageBucket = s3imageBucket
            };

            
            services.AddHttpClient<IImageUploader, AwsImageUploader>()
                        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddHttpClient();
            services.AddSingleton<S3Settings>(s3settingObj);
            services.AddScoped<IImageUploader, AwsImageUploader>();

            return services;
        }
    }
}
