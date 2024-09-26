using WordPressPCL;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.IO;
using System.Globalization;
using System.Net.Http.Headers;
using Common.Content.Interface;
using Amazon.Runtime.Internal.Util;

namespace BlogHelper.WordpressHelper
{
    /// <summary>
    /// Class responsible for uploading images to WordPress.
    /// </summary>
    public class UploadImagIntoWordpress : IImageUploader
    {

        private readonly string _fileName;
        WordPressClient _wordPressClient;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadImagIntoWordpress"/> class.
        /// </summary>
        /// <param name="wordPressClient">The WordPress client instance.</param>
        /// <param name="logger">The logger instance.</param>
        public UploadImagIntoWordpress( WordPressClient wordPressClient, ILogger logger)
        {   
            _wordPressClient = wordPressClient;
            _logger = logger;
        }

        /// <summary>
        /// Uploads an image to a specified bucket. (Not implemented yet)
        /// </summary>
        /// <param name="imagePath">The path of the image to upload.</param>
        /// <param name="bucketName">The name of the bucket to upload the image to.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="NotImplementedException">Thrown when the method is not implemented.</exception>
        public Task<string> UploadImage(string imagePath, string bucketName)
        {
            throw new NotImplementedException("yet to implement this :D");
        }

        /// <summary>
        /// Uploads an image from a URL to WordPress.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to upload.</param>
        /// <param name="SaveAsImageName">The name to save the image as.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response as a string.</returns>
        public async Task<string> UploadImageFromUrl(string imageUrl, string SaveAsImageName)
        {
            var returnresponse = await UploadImageToMediaLibraryAsync(_wordPressClient, imageUrl, SaveAsImageName);
            return returnresponse.ToString();
        }

        /// <summary>
        /// Uploads an image to the WordPress media library.
        /// </summary>
        /// <param name="client">The WordPress client instance.</param>
        /// <param name="imageUri">The URI of the image to upload.</param>
        /// <param name="imageName">The name to save the image as.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the uploaded media.</returns>
        /// <exception cref="Exception">Thrown when an error occurs while uploading the image.</exception>

        public async Task<int> UploadImageToMediaLibraryAsync(WordPressClient client, string imageUri, string imageName)
        {
            try
            {
                imageName = $"{imageName.Replace(" ", "_")}.png";
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(imageUri);

                    if (response.IsSuccessStatusCode)
                    {
                        string mimeType = response.Content.Headers.ContentType.MediaType;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var content = new StreamContent(stream);
                            content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

                            using (var streamReader = new StreamReader(await content.ReadAsStreamAsync()))
                            {
                                var uploadedMedia = await client.Media.CreateAsync(streamReader.BaseStream, imageName, mimeType);
                                return uploadedMedia?.Id ?? -1;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed to download image from '{imageUri}'.");
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadImagIntoWordpress.UploadImageToMediaLibraryAsync() unhandled exception");
                return -1;
            }
        }
    }
}
