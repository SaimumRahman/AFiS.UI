using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL
{
    public abstract class BaseRepository
    {
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ITokenProvider _tokenProvider;
        protected readonly ILogger _logger;

        protected BaseRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        protected HttpClient GetAuthenticatedClient(string clientName = "MainApi")
        {
            var client = _httpClientFactory.CreateClient(clientName);

            var token = _tokenProvider.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"✅ Token added to {clientName} client");
            }
            else
            {
                _logger.LogWarning($"⚠️ No token available for {clientName} client");
            }

            return client;
        }
    }
}
