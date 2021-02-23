
using System.Text.Json.Serialization;

namespace ButtBot.Website.Models
{
    public class UserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }
        
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
        
        [JsonPropertyName("bot")]
        public bool? IsBot { get; set; }
        
        [JsonPropertyName("system")]
        public bool? IsSystem { get; set; }
        
        [JsonPropertyName("mfa_enabled")]
        public bool? IsMfaEnabled { get; set; }
        
        [JsonPropertyName("locale")]
        public string? Locale { get; set; }
        
        [JsonPropertyName("verified")]
        public bool? Verified { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        
        [JsonPropertyName("flags")]
        public int? Flags { get; set; }
        
        [JsonPropertyName("premium_type")]
        public int? PremiumType { get; set; }
        
        [JsonPropertyName("public_flags")]
        public int? PublicFlags  { get; set; }
    }

    public class Connection
    {
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("revoked")]
        public bool? Revoked { get; set; }
        
        // [JsonProperty("integrations")]
        // public string integrations? { get; set; }
        
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("friend_sync")]
        public bool FriendSync { get; set; }
        
        [JsonPropertyName("show_activity")]
        public bool ShowActivity { get; set; }
        
        [JsonPropertyName("visibility")]
        public int Visibility { get; set; }
    }
}
