using System.ComponentModel.DataAnnotations;

namespace CoordinatorsAppAPI.AppSettings
{
    public class OAuthCredentials
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;
        [Required]
        public string ClientSecret { get; set; } = string.Empty;
    }
}
