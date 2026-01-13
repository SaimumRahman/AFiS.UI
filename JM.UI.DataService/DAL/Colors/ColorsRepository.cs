using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Colors;

public class ColorsRepository : BaseRepository, IColorsRepository
{
    public ColorsRepository(IHttpClientFactory factory, ITokenProvider token, ILogger<ColorsRepository> logger)
        : base(factory, token, logger) { }

    public async Task<IEnumerable<ColorsDTO>> GetColors()
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync("Colors/getall");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<ColorsDTO>>() ?? new();
    }

    public async Task<ColorsDTO?> GetColorsById(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync($"Colors/get/{id}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ColorsDTO>() : null;
    }

    public async Task<ResponseResult> SaveUpdateColors(ColorsDTO Colors)
    {
        var content = JsonContent.Create(new { ColorsDTO = Colors });
        var res = await GetAuthenticatedClient("MainApi").PostAsync("Colors/insert-update", content);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ResponseResult>() ?? new() { IsSuccessStatus = false };
    }

    public async Task DeleteColors(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").DeleteAsync($"Colors/delete/{id}");
        res.EnsureSuccessStatusCode();
    }

}
