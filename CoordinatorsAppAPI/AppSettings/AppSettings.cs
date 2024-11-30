using System.ComponentModel.DataAnnotations;

namespace CoordinatorsAppAPI.AppSettings
{
    public class AppSettings
    {
        public string Hello { get; set; } = string.Empty;
        [Required]
        public string FrontendUrl { get; set; } = string.Empty;
    }
}
