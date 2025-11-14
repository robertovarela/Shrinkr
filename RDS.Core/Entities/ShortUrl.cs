namespace RDS.Core.Entities
{
    public class ShortUrl
    {
        public long Id { get; init; } // ID interno, será usado para Hashids
        public string LongUrl { get; init; } = string.Empty; // A URL original
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow; // Timestamp da criação
        public int ClickCount { get; init; } = 0; // Opcional: para análise futura
    }
}