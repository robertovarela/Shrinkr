using Microsoft.AspNetCore.Http;
using RDS.Core.Dtos;

namespace RDS.Core.Interfaces;

public interface IUrlShorteningService
{
    /// <summary>
    /// Cria uma nova URL encurtada a partir de uma URL longa.
    /// </summary>
    /// <param name="longUrl">A URL longa a ser encurtada.</param>
    /// <param name="scheme">O esquema (ex: "https") da requisição atual.</param>
    /// <param name="host">O host (ex: "localhost:5115") da requisição atual.</param>
    /// <returns>Um objeto CreateShortUrlResult indicando sucesso ou falha.</returns>
    Task<CreateShortUrlResult> CreateShortUrlAsync(string longUrl, string scheme, string host);

    /// <summary>
    /// Processa a requisição de uma URL curta, retornando os detalhes para a resposta.
    /// </summary>
    /// <param name="shortCode">O código curto da URL.</param>
    /// <param name="headers">Os cabeçalhos da requisição HTTP.</param>
    /// <returns>Um objeto HandleShortUrlResponse contendo a URL longa e se o cliente prefere JSON,
    /// ou null se não for encontrado.</returns>
    Task<HandleShortUrlResponse?> HandleShortUrlRequestAsync(string shortCode, IHeaderDictionary headers);
}