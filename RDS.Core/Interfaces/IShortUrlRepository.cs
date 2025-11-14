using RDS.Core.Dtos;

namespace RDS.Core.Interfaces
{
    public interface IShortUrlRepository
    {
        Task<long> AddAsync(CreateShortUrlDto shortUrl);
        Task<ReadShortUrlDto?> GetByIdAsync(long id);
        Task UpdateAsync(UpdateShortUrlDto shortUrl);
        Task IncrementClickCountAsync(long id);
    }
}