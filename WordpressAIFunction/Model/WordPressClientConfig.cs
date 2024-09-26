using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordpressAIFunction.Model
{
    public class WordPressClientConfig
    {
        [JsonPropertyName("api_url")]
        public string ApiUrl { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        public string Password { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
