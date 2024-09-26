using Common.Content.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content.Interface
{
    public interface IContentGenerator
    {
        Task<string> GenerateBlogPostInPlainText(string promptText, string plainTextOutputTemplate);
        Task<Article> GenerateBlogPost(string promptText, string jsonOutputTemplate);
    }

    public interface IOpenAPIImageGenerator
    {
        Task<string> GenerateImageAsync(string prompt, int num_images);
    }
}
