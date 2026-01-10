using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Banks;

public class BanksRepository : BaseRepository, IBanksRepository
{
    public BanksRepository(IHttpClientFactory factory, ITokenProvider token, ILogger<BanksRepository> logger)
        : base(factory, token, logger) { }

    public async Task<IEnumerable<BanksDTO>> GetBankss()
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync("Banks/getall");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<BanksDTO>>() ?? new();
    }

    public async Task<BanksDTO?> GetBanksById(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync($"Banks/get/{id}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<BanksDTO>() : null;
    }

    public async Task<ResponseResult> SaveUpdateBanks(BanksDTO Banks)
    {
        var content = JsonContent.Create(new { BanksDTO = Banks });
        var res = await GetAuthenticatedClient("MainApi").PostAsync("Banks/insert-update", content);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ResponseResult>() ?? new() { IsSuccessStatus = false };
    }

    public async Task DeleteBanks(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").DeleteAsync($"Banks/delete/{id}");
        res.EnsureSuccessStatusCode();
    }

}
