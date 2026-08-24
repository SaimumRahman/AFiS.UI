using JM.UI.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JM.UI.Client.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public AuthTokenHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add the bearer token to the request headers
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // If we get a 401 (Unauthorized), try to refresh the token
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var refreshSuccessful = await _tokenService.RefreshAccessTokenAsync();

                if (refreshSuccessful)
                {
                    // Retry the original request with the new token
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await _tokenService.GetTokenAsync());
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}