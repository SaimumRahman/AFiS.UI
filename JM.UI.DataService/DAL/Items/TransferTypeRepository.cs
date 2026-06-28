using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public class TransferTypeRepository : BaseRepository, ITransferTypeRepository
    {
        public TransferTypeRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<TransferTypeRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        public async Task<IEnumerable<TransferTypeDTO>> GetTransferTypes()
        {
            try
            {
                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("Items/GetTransferTypes");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<TransferTypeDTO>>();
                return result ?? new List<TransferTypeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transfer types");
                throw;
            }
        }
    }
}
