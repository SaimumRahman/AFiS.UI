using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.StockOpening;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.StockOpenings
{
    public class StockOpeningRepository : BaseRepository, IStockOpeningRepository
    {
        private readonly ILogger<StockOpeningRepository> _logger;
        public StockOpeningRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<StockOpeningRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
            _logger = logger;
        }

        public async Task<ResponseResult> InsertStockOpening(StockOpeningEntryDTO stockOpening)
        {
            try
            {
                _logger.LogInformation("Starting to save stock opening");

                var httpClient = GetAuthenticatedClient("MainApi");
                var requestBody = new
                {
                    stockOpening = stockOpening
                };
                var content = JsonContent.Create(requestBody);
                var response = await httpClient.PostAsync("api/StockOpening/insert", content);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<ResponseResult>();
                return result ?? new ResponseResult { IsSuccessStatus = false, Message = "No response from server" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling insert stock opening API");
                return new ResponseResult { IsSuccessStatus = false, Message = "API call failed: " + ex.Message };
            }
        }

        public async Task<IEnumerable<StockOpeningEntryDTO>> GetAllStockOpenings()
        {
            try
            {
                _logger.LogInformation("Starting to fetch all stock openings");

                var httpClient = GetAuthenticatedClient("MainApi");
                var response = await httpClient.GetAsync("api/StockOpening/getall");
                response.EnsureSuccessStatusCode();

                var stockOpenings = await response.Content.ReadFromJsonAsync<List<StockOpeningEntryDTO>>();
                return stockOpenings ?? new List<StockOpeningEntryDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all stock openings");
                return new List<StockOpeningEntryDTO>();
            }
        }
    }
}
