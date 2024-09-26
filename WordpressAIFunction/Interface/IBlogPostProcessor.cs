using Common.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordpressAIFunction.Interface
{
    public interface IBlogPoster
    {
        Task<bool> PostToWordPressAsync(Article article);
        Task<int> SearchImageInTheMedia(string imageUri, string imageName);
        Task<string> UploadImageToMediaLibraryAsync(string imageUri, string imageName);
        Task<int> GetExistingCategoryAsync(string categoryName);
        Task<int> UpsertCategoryAsync(string categoryName, string categorySlug, string parentCategoryName);
    }


}
