namespace RDS.Core.Dtos;

public record UpdateShortUrlDto
{
    public long Id { get; set; }
    public int ClickCount { get; set; }
}