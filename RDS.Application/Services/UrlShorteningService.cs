using System.Diagnostics.CodeAnalysis;
using HashidsNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RDS.Core.Dtos;
using RDS.Core.Interfaces;

namespace RDS.Application.Services
{
    public class UrlShorteningService(
        IShortUrlRepository shortUrlRepository,
        ILogger<UrlShorteningService> logger,
        IHashids hashids) : IUrlShorteningService
    {
        public async Task<CreateShortUrlResult> CreateShortUrlAsync(string longUrl, string scheme, string host)
        {
            if (!await ValidateUrlAsync(longUrl))
            {
                return CreateShortUrlResult.Failure("A URL fornecida não é válida.");
            }

            var urlId = await AddLongUrlAsync(longUrl);
            if (urlId == 0)
            {
                return CreateShortUrlResult.Failure("Não foi possível salvar a URL no banco de dados.");
            }

            var shortCode = EncodeId(urlId);
            var shortUrl = $"{scheme}://{host}/{shortCode}";

            return CreateShortUrlResult.Success(shortUrl);
        }

        public async Task<HandleShortUrlResponse?> HandleShortUrlRequestAsync(string shortCode, IHeaderDictionary headers)
        {
            ReadShortUrlDto? url = null;
            long id = 0;
            try
            {
                var decodedIds = hashids.DecodeLong(shortCode);
                if (decodedIds.Length > 0)
                {
                    id = decodedIds[0];
                }

                if (id != 0)
                {
                    url = await shortUrlRepository.GetByIdAsync(id);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erro ao decodificar o shortCode '{ShortCode}' ou buscar a URL. Pode ser um shortCode inválido.", shortCode);
                return null;
            }

            if (url == null)
            {
                return null;
            }

            _ = shortUrlRepository.IncrementClickCountAsync(id);

            var wantsJson =
                (headers.TryGetValue("Accept", out var accept) && accept.ToString()
                    .Contains("application/json", StringComparison.OrdinalIgnoreCase))
                || (headers.TryGetValue("X-Requested-With", out var xrw) &&
                    xrw.ToString().Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                || (headers.TryGetValue("sec-fetch-mode", out var sfm) &&
                    sfm.ToString().Equals("cors", StringComparison.OrdinalIgnoreCase))
                || (headers.TryGetValue("Referer", out var referer) &&
                    referer.ToString().Contains("/swagger", StringComparison.OrdinalIgnoreCase))
                || (headers.ContainsKey("Origin"));

            return new HandleShortUrlResponse
            {
                LongUrl = url.LongUrl,
                WantsJson = wantsJson
            };
        }

        // Métodos auxiliares privados
        private Task<bool> ValidateUrlAsync(string longUrl)
        {
            return Task.FromResult(Uri.TryCreate(longUrl, UriKind.Absolute, out _));
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private async Task<long> AddLongUrlAsync(string longUrl)
        {
            try
            {
                var newUrl = new CreateShortUrlDto { LongUrl = longUrl };
                var addedUrlId = await shortUrlRepository.AddAsync(newUrl);

                if (addedUrlId == 0)
                {
                    logger.LogError("Falha ao salvar a URL e obter um ID. {LongUrl}", longUrl);
                }
                return addedUrlId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocorreu um erro inesperado ao criar a URL curta para {LongUrl}", longUrl);
                return 0;
            }
        }

        private string EncodeId(long id)
        {
            return hashids.EncodeLong(id);
        }
    }
}