using System.ComponentModel.DataAnnotations;

namespace RDS.API.Contracts.Requests
{
    public class ShortenUrlRequest
    {
        [Required]
        public string LongUrl { get; set; } = string.Empty;
    }
}