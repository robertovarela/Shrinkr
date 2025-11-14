namespace RDS.Core.Dtos
{
    public class HandleShortUrlResponse
    {
        public string LongUrl { get; set; } = string.Empty;
        public bool WantsJson { get; set; }
    }
}