namespace RDS.Core.Dtos;

public record ReadShortUrlDto
{
    public required string LongUrl { get; init; }
}