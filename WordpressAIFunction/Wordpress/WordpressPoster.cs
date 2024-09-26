using WordPressPCL;
using WordPressPCL.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using WordPressPCL.Utility;
using System.Linq;
using Common.Content.Model;
using Common.Content.Interface;
using Microsoft.Extensions.Logging;
using WordpressAIFunction.Interface;
using WordpressAIFunction.Model;

namespace WordpressAIFunction.Wordpress
{
    /// <summary>
    /// Class responsible for posting content to a WordPress blog.
    /// </summary>
    public class WordPressPoster : IBlogPoster
    {
        private readonly string _ParentCategoryName;
        private IImageUploader _imageUplader;
        private ILogger<WordPressPoster> _logger;
        private WordPressClient _wordpressClient;
        private readonly WordPressClientConfig _wordpressClientConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WordPressPoster"/> class.
        /// </summary>
        /// <param name="wordpressClientConfig">The WordPress client configuration.</param>
        /// <param name="imageUplader">The image uploader service.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="TypeInitializationException">Thrown when the WordPress client configuration is null.</exception>
        public WordPressPoster(WordPressClientConfig wordpressClientConfig, IImageUploader imageUplader, ILogger<WordPressPoster> logger)
        {
            _ = wordpressClientConfig
                ?? throw new TypeInitializationException(nameof(WordPressPoster), new ArgumentNullException($"WordPressPoster Init failed. {nameof(wordpressClientConfig)} is null"));
            _wordpressClientConfig = wordpressClientConfig;
            _ParentCategoryName = "Look Beauty Blog";
            _imageUplader = imageUplader;
            _logger = logger;
        }

        /// <summary>
        /// Gets the WordPress client instance.
        /// </summary>
        private WordPressClient WordPressClient
        {
            get
            {
                try
                {
                    if (_wordpressClient == null)
                    {
                        _wordpressClient = new WordPressClient(_wordpressClientConfig.ApiUrl);
                        WordPressClient.Auth.UseBasicAuth(_wordpressClientConfig.Username, _wordpressClientConfig.Password);
                    }
                    return _wordpressClient;
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"WordPressClient Instance creation failed with exception. Check configuration {_wordpressClientConfig?.ToString()}.", ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// Posts an article to the WordPress blog.
        /// </summary>
        /// <param name="article">The article to post.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
        public async Task<bool> PostToWordPressAsync(Article article)
        {
            try
            {
                var post = new Post
                {
                    Title = new Title(article.title),
                    Content = new Content(article.body),
                    Status = Status.Publish
                };

                if (!string.IsNullOrWhiteSpace(article.imageUrl))
                {
                    var imageurl = await UploadImageToMediaLibraryAsync(article.imageUrl, $"{article.title}");
                }

                var createdPost = await WordPressClient.Posts.CreateAsync(post);
                if (createdPost == null)
                {
                    return false;
                }

                var tagIds = await CreateTagsAsync(article.tags);
                createdPost.Tags = tagIds;

                var categoryId = await UpsertCategoryAsync(article.category, article.category.Replace(" ", "-"), _ParentCategoryName);

                if (categoryId > 0)
                    createdPost.Categories = new List<int> { categoryId };

                var updatedPost = await WordPressClient.Posts.UpdateAsync(createdPost);
                return updatedPost != null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PostToWordPressAsync() failed.");
                return false;
            }
        }

        /// <summary>
        /// Creates tags in WordPress if they do not already exist.
        /// </summary>
        /// <param name="tags">The tags to create.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of tag IDs.</returns>
        private async Task<List<int>> CreateTagsAsync(string[] tags)
        {
            try
            {
                WordPressClient client = WordPressClient;
                var tagIds = new List<int>();
                var existingTags = await client.Tags.GetAllAsync();

                foreach (var tag in tags)
                {
                    if (string.IsNullOrWhiteSpace(tag))
                        continue;
                    var tagName = tag.Trim();
                    var tagSlug = tagName.ToLower().Replace(" ", "-");

                    var existingTag = existingTags.FirstOrDefault(t => t.Name == tagName || t.Slug == tagSlug);

                    if (existingTag != null)
                    {
                        tagIds.Add(existingTag.Id);
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = tagName,
                            Slug = tagSlug
                        };

                        var createdTag = await client.Tags.CreateAsync(newTag);
                        tagIds.Add(createdTag.Id);
                    }
                }
                return tagIds;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "CreateTagsAsync() failed.");
                return null;
            }
        }

        /// <summary>
        /// Searches for an image in the WordPress media library.
        /// </summary>
        /// <param name="imageUri">The URI of the image.</param>
        /// <param name="imageName">The name of the image.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the image ID if found, otherwise -1.</returns>
        public async Task<int> SearchImageInTheMedia(string imageUri, string imageName)
        {
            WordPressClient client = WordPressClient;
            var queryBuilder = new MediaQueryBuilder
            {
                Search = imageName
            };
            var existingMediaItems = await client.Media.QueryAsync(queryBuilder);
            var existingMedia = existingMediaItems.FirstOrDefault(m => m.Title.Rendered.Equals(imageName, StringComparison.OrdinalIgnoreCase));
            return existingMedia?.Id ?? -1;
        }

        /// <summary>
        /// Uploads an image to the WordPress media library.
        /// </summary>
        /// <param name="imageUri">The URI of the image.</param>
        /// <param name="imageName">The name of the image.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the URL of the uploaded image.</returns>
        public async Task<string> UploadImageToMediaLibraryAsync(string imageUri, string imageName)
        {
            try
            {
                WordPressClient client = WordPressClient;
                var imageurl = await _imageUplader?.UploadImageFromUrl(imageUri, imageName) ?? string.Empty;
                return imageurl;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UploadImageToMediaLibraryAsync() failed.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the ID of an existing category in WordPress.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the category ID if found, otherwise -1.</returns>
        public async Task<int> GetExistingCategoryAsync(string categoryName)
        {
            try
            {
                WordPressClient client = WordPressClient;
                var queryBuilder = new CategoriesQueryBuilder
                {
                    Search = categoryName
                };
                var categories = await client.Categories.QueryAsync(queryBuilder);
                var foundCategory = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                return foundCategory?.Id ?? -1;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetExistingCategoryAsync() failed.");
                return -1;
            }
        }

        /// <summary>
        /// Creates or updates a category in WordPress.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <param name="categorySlug">The slug of the category.</param>
        /// <param name="parentCategoryName">The name of the parent category.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the category ID.</returns>
        public async Task<int> UpsertCategoryAsync(string categoryName, string categorySlug, string parentCategoryName)
        {
            try
            {
                WordPressClient client = WordPressClient;
                var existingCategoryId = await GetExistingCategoryAsync(categoryName);
                if (existingCategoryId > 0)
                {
                    return existingCategoryId;
                }
                else
                {
                    var parentcategoryId = await GetExistingCategoryAsync(parentCategoryName);
                    var newCategory = new Category
                    {
                        Name = categoryName,
                        Slug = categorySlug,
                        Parent = parentcategoryId > 0 ? parentcategoryId : 0
                    };

                    var createdCategory = await client.Categories.CreateAsync(newCategory);
                    return createdCategory.Id;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UpsertCategoryAsync() failed.");
                return -1;
            }
        }
    }
}
