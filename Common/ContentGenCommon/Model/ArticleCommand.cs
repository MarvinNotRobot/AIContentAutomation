using Newtonsoft.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Guid = System.Guid;
using Newtonsoft.Json;


namespace Common.Content.Model
{
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

        public static Article ParaseJsonParser(string jsonInput)
        {
            if (string.IsNullOrEmpty(jsonInput))
                return null;
            Article remedy = JsonConvert.DeserializeObject<Article>(Regex.Unescape(jsonInput));
            return remedy;
        }
    }
}
