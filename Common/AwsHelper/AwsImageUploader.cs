using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Common.AWS.Model;
using Common.Content.Interface;

namespace Common.AWS
{
    public class AwsImageUploader : IImageUploader
    {
        private readonly S3Settings _s3Settings;
        private readonly HttpClient _httpClient;

        public AwsImageUploader(S3Settings s3Settings, HttpClient httpClient)
        {
            _s3Settings = s3Settings;
            _httpClient = httpClient;
        }

        public async Task<string> UploadImage(string imagePath, string bucketName)
        {
            using (var client = new AmazonS3Client(_s3Settings.AccessKey, _s3Settings.SecretKey, _s3Settings.Region))
            {
                var imageKey = GetImageKey(imagePath);

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = imageKey,
                    FilePath = imagePath
                };

                await client.PutObjectAsync(request);

                return $"https://{_s3Settings.imageBucket}.s3.amazonaws.com/{imageKey}";

            }
        }

        public async Task<string> UploadImageFromUrl(string imageUrl, string imageName)
        {
            try
            { 
                var objectKey = GetUniqueFileName(imageName);


                using (var response = await _httpClient.GetAsync(imageUrl))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Failed to download the image from the URL.");
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var s3client = new AmazonS3Client(_s3Settings.AccessKey, _s3Settings.SecretKey, Amazon.RegionEndpoint.USEast1))
                        {
                            var transferUtility = new TransferUtility(s3client);
                            var request = new TransferUtilityUploadRequest
                            {
                                BucketName = _s3Settings.imageBucket,
                                Key = objectKey,
                                InputStream = stream,
                                CannedACL = S3CannedACL.PublicRead, // Optional: Set the ACL to grant public read access
                            };
                           await transferUtility.UploadAsync(request);

                        }
                    }
                }
                return $"https://{_s3Settings.imageBucket}.s3.amazonaws.com/{objectKey}";
            }
            catch (Exception ex)
            {
                // Handle the exception
                throw;
            }
        }

        private string GetUniqueFileName(string ImageNameWithExtensions)
        {
            return $"{_s3Settings.imageUploadPath}/" + Guid.NewGuid().GetHashCode() + $"{ImageNameWithExtensions}";
        }

        private string GetImageFileNameFromUrl(string imageUrl)
        {
            string fileNameWithExtension = imageUrl.Substring(imageUrl.LastIndexOf("/") + 1);
            return fileNameWithExtension;
        }

        private string GetImageKey(string imagePath)
        {
            // Generate a unique key for the image in the S3 bucket
            // You can use a custom logic here based on your requirements
            return $"{_s3Settings.imageUploadPath}/" + Guid.NewGuid().GetHashCode() + Path.GetExtension(imagePath);
        }
    }
}       
   
