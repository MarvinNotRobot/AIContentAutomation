using Newtonsoft.Json.Serialization;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenAI_API.Images;
using System.Drawing;
using WordPressPCL.Models;
using Guid = System.Guid;
using System.Net.NetworkInformation;
using WordPressPCL.Client;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Article
{
    [JsonProperty("title")]
    public string title { get; set; }
    [JsonProperty("body")]
    public string body { get; set; }
    [JsonProperty("tags")]
    public string[] tags { get; set; }
    [JsonProperty("imageUrl")]
    public string imageUrl { get; set; }
    [JsonProperty("category")]
    public string category { get; set; }

    [JsonProperty("summary")]
    public string summary { get; set; }

    //public static Article ParseTextResponse(string response)
    //{
    //   // var promptText = $"Topic:\n{topicHeader}.~TopicEndsHere~\n\nTitle:{{title}}\nSummary:{{summary}}\nCategory:{{category}}\nBody:{{body}}\nTags:{{tags}}";
    //    int titleStart = response.IndexOf("Title:") + "Title:".Length;
    //    int bodyStart = response.IndexOf("Body:") + "Body:".Length;
    //    int tagsStart = response.IndexOf("Tags:") + "Tags:".Length;
    //    int categoryStart = response.IndexOf("Category:") + "Tags:".Length;
    //    int summaryStart = response.IndexOf("Summary:") + "Tags:".Length;

    //    int titleLength = summaryStart - "Summary:".Length - titleStart;
    //    int SummaryLength = categoryStart - "Category:".Length - titleLength;
    //    int CategoryLength = bodyStart - "Body:".Length - SummaryLength;
    //    int bodyLength = tagsStart - "Tags:".Length - CategoryLength;

    //    string title = response.Substring(titleStart, titleLength).Trim();
    //    string body = response.Substring(bodyStart, bodyLength).Trim();
    //    string category = response.Substring(categoryStart, CategoryLength).Trim();
    //    string summary = response.Substring(summaryStart, SummaryLength).Trim();

    //    string[] tags = Regex.Split(response.Substring(tagsStart).Trim(), @"\s*[,#]\s*").Select(tag => tag.Trim()).Take(8).ToArray();
        
    //    return new Article
    //    {
    //        Title = title,
    //        Body = body,
    //        Tags = tags,
    //        Category = category,
    //        Summary = summary
    //    };
    //}

    //public static Article ParaseXmlParser(string xml)
    //        {
    //            if (string.IsNullOrEmpty(xml))
    //                return null;

    //            Article article = new Article();
    //            xml = xml.Replace("\n", "").Replace("\r", "").Replace("\t", "");

    //            XDocument doc = XDocument.Parse(xml);

    //            XElement root = doc.Root;
    //            XElement titleElement = root.Element("t");
    //            if (titleElement != null)
    //            {
    //                article.Title = titleElement.Value.Trim();
    //            }

    //            XElement summaryElement = root.Element("s");
    //            if (summaryElement != null)
    //            {
    //                article.Summary = summaryElement.Value.Trim();
    //            }

    //            XElement categoryElement = root.Element("c");
    //            if (categoryElement != null)
    //            {
    //                article.Category = categoryElement.Value.Trim();
    //            }

    //            XElement bodyElement = root.Element("b");
    //            if (bodyElement != null)
    //            {
    //                article.Body = bodyElement.Value.Trim();
    //            }

    //            XElement tagsElement = root.Element("a");
    //            if (tagsElement != null)
    //            {
    //                article.Tags = Regex.Split(tagsElement.Value.Trim(), @"\s*[,#]\s*").Select(tag => tag.Trim()).Take(8).ToArray();  ;
    //            }

    //            return article;
    //        }

    public static Article ParaseJsonParser(string jsonInput)
    {
        if (string.IsNullOrEmpty(jsonInput))
            return null;
        Article remedy = JsonConvert.DeserializeObject<Article>(Regex.Unescape(jsonInput));
        return remedy;
    }
}



public class BlogPostGenerator
{
    private readonly OpenAIAPI _openaiClient;

    private string _orgname;
    private string _apikey;
    public BlogPostGenerator(string apiKey)
    {

         _apikey = "sk - QXOjRiEBGEzCOztyzXUpT3BlbkFJD1yL7JO3drdVtCjGlQln";
         _openaiClient = new OpenAIAPI(new APIAuthentication(apiKey, "org-KBxDgV4esuKb6YTQJjWF8PsF"));
    }


    public async Task<Article> GenerateBlogPost(string topicHeader)
    {
        try
        {
            // Set up the prompt for the OpenAI API request
            //var promptText = $"Generate an SEO optimized 'at home' DIY natural remedy  blog post with a title, body, and tags using the following information:\n{topicHeader}~TopicEndsHere~\n\nTitle:{{title}}\nBody:{{body}}\nTags:{{tags}}";
            //var promptText = $"Generate an SEO optimized 'at home' DIY natural remedy blog post with a title, body, category, summary and tags.Category must be more specific and relevant to blog past. Summary should highlight remedy ingredient.Dont change the output format.Topic:\n{topicHeader} ~TopicEndsHere~\n\n<root><t>{{title}}</t><s>{{summary}}</s><c>{{category}}</c><b>{{body}}</b><a>{{tags}}</a></root>";
            var promptText = $"Generate an SEO optimized do it yourself at home organic natural remedy blog post with a title, body, category, summary and tags. category must be more specific and relevant to blog past. summary should highlight remedy ingredient. Make article sounds excited happy, professional , funny and friendly targeting millennials and gen z readeres.  Answer in JSON machine-readable format, use following keys title,body,category,summary and tags.Topic to write:\n{topicHeader} ~TopicEndsHere~";
            var request = new CompletionRequest(
            prompt: promptText,
            model: Model.DavinciText,
            max_tokens: 3500,
            temperature: 0.7, //temperature
            stopSequences: "~TopicEndsHere~",
            numOutputs: 1
            );

            var result = await _openaiClient.Completions.CreateCompletionAsync(request);
           
            var response = result.Completions.First().Text.Trim();
            var article = Article.ParaseJsonParser(response);

             article.imageUrl = await GenerateImageAsync(article.summary, 1);
            return article;
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
            return null;
        }

    }
    public async Task<string> GenerateImageAsync(string prompt, int num_images)
    {

        if (string.IsNullOrEmpty(prompt))
            return string.Empty;

        try
        {
            //new ImageGenerationRequest("A drawing of a computer writing a test", 1, ImageSize._512)
            var requestOptions = new ImageGenerationRequest
            {
                NumOfImages = num_images,
                ResponseFormat = ImageResponseFormat.Url,
                Prompt = prompt,
                User = $"nkuser_{Guid.NewGuid().ToString()}",
                Size = ImageSize._1024
            };
            
            var imageResponse = await _openaiClient.ImageGenerations.CreateImageAsync(requestOptions);

            if ((imageResponse?.Data?.Count ?? 0) <= 0 || string.IsNullOrWhiteSpace((imageResponse.Data.First().Url)))
            {
                Console.WriteLine($"Error: empty image returned by proivder");
                return null;
            }

            return imageResponse.Data.First().Url;

        }
        catch (Exception e)
        {
            Console.WriteLine($"GenerateImageAsync().Error: {e.Message}");
            return null; 
        }
    }
}
