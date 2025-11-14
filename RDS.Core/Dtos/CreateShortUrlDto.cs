namespace RDS.Core.Dtos;

public record CreateShortUrlDto
{
    public required string LongUrl { get; init; }
}