using Microsoft.AspNetCore.Mvc;
using RDS.API.Contracts.Requests;
using RDS.API.Contracts.Responses;
using RDS.Core.Interfaces;

namespace RDS.API.Controllers
{
    [ApiController]
    [Route("")]
    public class UrlShortenerController(IUrlShorteningService urlShorteningService) : ControllerBase
    {
        /// <summary>
        /// Cria uma nova URL encurtada.
        /// </summary>
        /// <param name="request">O objeto contendo a URL longa a ser encurtada.</param>
        /// <returns>A URL encurtada completa.</returns>
        [HttpPost]
        [Route("shorten")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateShortUrl([FromBody] ShortenUrlRequest request)
        {
            var result = await urlShorteningService.CreateShortUrlAsync(request.LongUrl,
                Request.Scheme, Request.Host.ToString());

            return result switch
            {
                { IsSuccess: true, ShortUrl: not null } => Ok(result.ShortUrl),
                _ => BadRequest(result.ErrorMessage)
            };
        }

        /// <summary>
        /// Redireciona ou retorna a URL longa original a partir de um código curto.
        /// Se o header 'Accept' contiver 'application/json', retorna o JSON.
        /// Caso contrário, redireciona o navegador.
        /// </summary>
        /// <param name="shortCode">O código da URL encurtada.</param>
        [HttpGet]
        [Route("{shortCode}")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(LongUrlResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> HandleShortUrl([FromRoute] string shortCode)
        {
            var response = await urlShorteningService.HandleShortUrlRequestAsync(shortCode, Request.Headers);

            return response switch
            {
                null => NotFound(),
                { WantsJson: true } => Ok(new LongUrlResponse { LongUrl = response.LongUrl }),
                _ => Redirect(response.LongUrl)
            };
        }
    }
}