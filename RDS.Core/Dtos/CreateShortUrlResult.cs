namespace RDS.Core.Dtos
{
    public class CreateShortUrlResult
    {
        public bool IsSuccess { get; private init; }
        public string? ShortUrl { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static CreateShortUrlResult Success(string shortUrl) =>
            new() { IsSuccess = true, ShortUrl = shortUrl };
        public static CreateShortUrlResult Failure(string errorMessage) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage };
    }
}