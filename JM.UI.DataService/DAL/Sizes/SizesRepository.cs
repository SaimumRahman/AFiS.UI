using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Sizes;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Sizes;

public class SizesRepository : BaseRepository, ISizesRepository
{
    public SizesRepository(IHttpClientFactory factory, ITokenProvider token, ILogger<SizesRepository> logger)
        : base(factory, token, logger) { }

    public async Task<IEnumerable<SizesDTO>> GetSizess()
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync("Sizes/getall");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<SizesDTO>>() ?? new();
    }

    public async Task<SizesDTO?> GetSizesById(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync($"Sizes/get/{id}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<SizesDTO>() : null;
    }

    public async Task<ResponseResult> SaveUpdateSizes(SizesDTO Sizes)
    {
        var content = JsonContent.Create(new { SizesDTO = Sizes });
        var res = await GetAuthenticatedClient("MainApi").PostAsync("Sizes/insert-update", content);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ResponseResult>() ?? new() { IsSuccessStatus = false };
    }

    public async Task DeleteSizes(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").DeleteAsync($"Sizes/delete/{id}");
        res.EnsureSuccessStatusCode();
    }

}
