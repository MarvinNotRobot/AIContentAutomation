using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.ShopifyAPI.Interface;
using Common.Content.Interface;
using Common.Content.Model;
using Microsoft.Extensions.Logging;
using Common.OpenAPI;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace ShopifyAIFunction
{
    public class ShopifyContentProcessHandler : IProcessHandler<int, string>
    {
        private readonly ILogger<ShopifyContentProcessHandler> _logger;
        private readonly IShopifyProductService _shopifyService;
        private readonly IContentGenerator _openAPIContentGenerator;
        private readonly string _chatGPTCommandPromptWithJsonFormat;
        private readonly string _chatGPTCommandPromptWithPlainTextFormat;

        public ShopifyContentProcessHandler(ILogger<ShopifyContentProcessHandler> logger, IShopifyProductService shopifyService, IContentGenerator openAPIContentGenerator, string jsonFormat, string plainTextFormat)
        {
            _logger = logger;
            _shopifyService = shopifyService;
            _openAPIContentGenerator = openAPIContentGenerator;
            _chatGPTCommandPromptWithJsonFormat = jsonFormat;
            _chatGPTCommandPromptWithPlainTextFormat = plainTextFormat;
        }

        public async Task<string> ProcessRequest(int goBackDays)
        {
            var responseMessage = "OK";
            var shopifyProducts = await _shopifyService.ListProducts(DateTime.Now.AddDays(-1 * goBackDays), DateTime.Now);

            foreach (var product in shopifyProducts)
            {
                try
                {
                    var plainText = RemoveHtmlTags(product.BodyHtml);
                    var output = await GetRewrittenArticle(plainText);

                    if (output == null)
                    {
                        _logger.LogError($"Rewrite failed. Product Title: {product.Title} Rewrite output had null output");
                        continue;
                    }

                    product.BodyHtml = output.body;
                    await _shopifyService.UpdateProduct((long)product.Id, product.BodyHtml, product.Title, null, output.tags);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while processing product ID: {product.Id}");
                }
            }

            return responseMessage;
        }

        private string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            string bodyHtmlWithoutTags = Regex.Replace(input, @"<[^>]+>|&nbsp;", "").Trim();
            return RemoveEmojis(bodyHtmlWithoutTags);
        }

        // fix unit test

        private static string RemoveEmojis(string input)
        {
            Regex emoji2Regex = new Regex(@"[\uD83C-\uDBFF\uDC00-\uDFFF]+");
            Regex emojiRegex = new Regex(@"[\p{So}\p{Sk}]");

            if (string.IsNullOrWhiteSpace(input))
                return null;
            
            var output =  emojiRegex.Replace(input, "");

            // Regular expression to match emojis
             return emoji2Regex.Replace(output, "");

        }

        private async Task<Article> GetRewrittenArticle(string productBodyHTML)
        {
            try
            {
                return await _openAPIContentGenerator.GenerateBlogPost(productBodyHTML, _chatGPTCommandPromptWithJsonFormat);
            }
            catch (OpenAIPromptTooLongException)
            {
                var summarizedContent = await SummarizeArticle(productBodyHTML, 100);
                return summarizedContent != null ? await _openAPIContentGenerator.GenerateBlogPost(summarizedContent.body, _chatGPTCommandPromptWithJsonFormat) : null;
            }
        }

        private async Task<Article> SummarizeArticle(string productBodyHTML, int size)
        {
            try
            {
                var shortPrompt = $"Act as a professional content rewriter. Summarize this content within {size} words without losing original context.";
                var output = await _openAPIContentGenerator.GenerateBlogPostInPlainText(productBodyHTML, shortPrompt);
                if ((output?.Length ??0) <= 0)
                    return null;
                return new Article { body = output };
            }
            catch (OpenAIPromptTooLongException)
            {
                if (productBodyHTML.Length <= 1000)
                    return null;

                var reducedLength = productBodyHTML.Substring(0, 1000);
                return await SummarizeArticle(reducedLength, size);
            }
        }
    }
}
