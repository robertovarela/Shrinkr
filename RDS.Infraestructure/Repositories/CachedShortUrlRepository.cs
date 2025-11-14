using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RDS.Core.Dtos;
using RDS.Core.Entities;
using RDS.Core.Interfaces;

namespace RDS.Infraestructure.Repositories
{
    // Decorator que adiciona cache baseado em IMemoryCache para GetByIdAsync.
    public class CachedShortUrlRepository(
        ShortUrlRepository inner,
        IMemoryCache cache,
        ILogger<CachedShortUrlRepository> logger)
        : IShortUrlRepository
    {
        // Opções de cache
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        private static string CacheKey(long id) => $"ShortUrl:{id}";

        public async Task<long> AddAsync(CreateShortUrlDto shortUrl)
        {
            var added = await inner.AddAsync(shortUrl);
            if (added != 0)
            {
                logger.LogDebug("ShortUrl id={Id} added and cached", added);
                cache.Set(CacheKey(added), added, _cacheOptions);
            }
            return added;
        }

        public async Task<ReadShortUrlDto?> GetByIdAsync(long id)
        {
            if (cache.TryGetValue<ReadShortUrlDto>(CacheKey(id), out var cached))
            {
                logger.LogDebug("Cache hit for ShortUrl id={Id}", id);
                return cached;
            }

            logger.LogDebug("Cache miss for ShortUrl id={Id}, fetching from database", id);
            var fromDb = await inner.GetByIdAsync(id);
            if (fromDb != null)
            {
                logger.LogDebug("ShortUrl id={Id} found in database, caching for 5 minutes", id);
                cache.Set(CacheKey(id), fromDb, _cacheOptions);
            }
            else
            {
                // Evite sobrecarga de cache para chaves ausentes: armazene o marcador nulo em cache por um curto período.
                logger.LogDebug("ShortUrl id={Id} not found in database, caching null for 30 seconds", id);
                cache.Set(CacheKey(id), null as ShortUrl, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                });
            }

            return fromDb;
        }

        public async Task UpdateAsync(UpdateShortUrlDto shortUrl)
        {
            await inner.UpdateAsync(shortUrl);
            // Atualizar cache
            logger.LogDebug("ShortUrl id={Id} updated and cache refreshed", shortUrl.Id);
            cache.Set(CacheKey(shortUrl.Id), shortUrl, _cacheOptions);
        }

        public async Task IncrementClickCountAsync(long id)
        {
            await inner.IncrementClickCountAsync(id);
            // Invalidar o cache para evitar inconsistências.
            // Na próxima leitura, será buscado do banco com o valor atualizado.
            logger.LogDebug("ShortUrl id={Id} click count incremented, cache invalidated", id);
            cache.Remove(CacheKey(id));
        }
    }
}

