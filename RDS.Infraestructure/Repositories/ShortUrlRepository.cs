using Microsoft.EntityFrameworkCore;
using RDS.Core.Dtos;
using RDS.Core.Entities;
using RDS.Core.Interfaces;

namespace RDS.Infraestructure.Repositories
{
    public class ShortUrlRepository(ApplicationDbContext context) : IShortUrlRepository
    {
        public async Task<long> AddAsync(CreateShortUrlDto url)
        {
            var shortUrl = new ShortUrl { LongUrl = url.LongUrl };
            await context.ShortUrls.AddAsync(shortUrl);
            await context.SaveChangesAsync();
            return shortUrl.Id;
        }

        public async Task<ReadShortUrlDto?> GetByIdAsync(long id)
        {
            // Carrega somente a URL Longa em vez de trazer o registro completo
            var result = await context.ShortUrls
                .Where(s => s.Id == id)
                .Select(s => new ReadShortUrlDto { LongUrl = s.LongUrl })
                .FirstOrDefaultAsync();
            
            return result;
        }

        public async Task UpdateAsync(UpdateShortUrlDto shortUrl)
        {
            context.Entry(shortUrl).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task IncrementClickCountAsync(long id)
        {
            // Atualiza por Id de forma performática sem carregar a entidade em memória.
            await context.ShortUrls
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(updates =>
                    updates.SetProperty(
                        s => s.ClickCount,
                        s => s.ClickCount + 1
                    )
                );
        }
    }
}