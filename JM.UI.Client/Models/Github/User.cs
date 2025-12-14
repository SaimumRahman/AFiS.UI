using System.Text.Json.Serialization;

namespace RadzenBlazorDemos.Server.Models.GitHub
{
    public class User
    {
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }
    }
}