using Guid = System.Guid;
using Common.Content.Model;
using Common.Content.Interface;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;
using Microsoft.Extensions.Logging;


namespace  Common.OpenAPI
{
   
    public class OpenAPIContentGenerator : IContentGenerator, IOpenAPIImageGenerator
    {
        private readonly OpenAIClient _openaiClient;

        private string _orgname;
        private string _apikey;
        ILogger _logger;
        public OpenAPIContentGenerator(string apiKey, string Org, ILogger logger)
        {
            _orgname = Org;
            _apikey = apiKey;
            _openaiClient = new OpenAIClient(new OpenAIAuthentication(apiKey, _orgname));
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="promptText"></param>
        /// <param name="jsonOutputTemplate">Example: Act as successfull content writer for ecommerce site. Generate a compelling and engaging product description for a makeup or beauty care item, focusing on its unique features, benefits, and ingredients. Highlight the product's effectiveness for various skin types and preferences. Craft the description in a way that resonates with the target audience and enhances product . Close the description with a persuasive call-to-action that entices potential customers to purchase the product and experience its transformative effects.\r\nThe target audience would be genZ and millionals. Answer in JSON machine-readable format, use following keys 'title' , 'body', 'tags'. 'title' must be crisp and catchy . 'body' should be easy to read and formatted with  'p', 'em', 'h3', 'strong','span'. 'body' can emojis.</param>
        /// <returns></returns>
        /// <exception cref="OpenAIPromptTooLongException"></exception>

        public async Task<Article> GenerateBlogPost(string subjectPromptText, string commandPromptWithjsonOutputInstruction)
        {
            try
            {
                var messages = new List<Message>
                    {
                        new Message(Role.System,commandPromptWithjsonOutputInstruction),
                        new Message(Role.User, subjectPromptText),
                    };
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT4o, responseFormat: ChatResponseFormat.Json, maxTokens: 3500, temperature:0.1, number:1);
                var response = await _openaiClient.ChatEndpoint.GetCompletionAsync(chatRequest);

                var output = response.Choices.First().Message.ToString();
                var article = Article.ParaseJsonParser(output);
                return article;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Please reduce your prompt"))
                {
                    throw new OpenAIPromptTooLongException("Prompt is very long", ex) { ErrorCode = OpenAIExceptionCode.Prompt_Too_Long };
                }
                Console.Write(ex.ToString());
                return null;
            }
        }

        public async Task<string> GenerateBlogPostInPlainText(string subjectPromptText, string plainTextOutputTemplate)
        {
            try
            {
                var messages = new List<Message>
                    {
                        new Message(Role.System, plainTextOutputTemplate),
                        new Message(Role.User, subjectPromptText),
                    };
                var chatRequest = new ChatRequest(messages, OpenAI.Models.Model.GPT4o, responseFormat: ChatResponseFormat.Text, maxTokens: 3500, temperature: 0.1, number: 1);
                var response = await _openaiClient.ChatEndpoint.GetCompletionAsync(chatRequest);

                var output = response.Choices.First().Message.ToString();
                return output;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Please reduce your prompt"))
                {
                    throw new OpenAIPromptTooLongException("Prompt is very long", ex) { ErrorCode = OpenAIExceptionCode.Prompt_Too_Long };
                }
                Console.Write(ex.ToString());
                return null;
            }
        }

        public async Task<string> GenerateImageAsync(string inputPrompt, int num_images)
        {
            if (string.IsNullOrEmpty(inputPrompt))
                return null;

            try
            {
                var request = new ImageGenerationRequest(prompt: inputPrompt, model: OpenAI.Models.Model.DallE_3, size: "1024x1024", user: $"nkuser_{Guid.NewGuid().ToString()}", numberOfResults: num_images, responseFormat: ImageResponseFormat.Url);
               
                var imageResponse = await _openaiClient.ImagesEndPoint.GenerateImageAsync(request);

                if (string.IsNullOrWhiteSpace(imageResponse?.First().Url))
                {
                    Console.WriteLine($"Error: empty image returned by proivder");
                    return null;
                }
                return imageResponse.First().Url;
            }
            catch (Exception e)
            {
                Console.WriteLine($"GenerateImageAsync().Error: {e.Message}");
                return null;
            }
        }
        
    }
}