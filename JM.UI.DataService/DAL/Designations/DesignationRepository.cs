using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Designations;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Designations;

public class DesignationRepository : BaseRepository, IDesignationRepository
{
    public DesignationRepository(IHttpClientFactory factory, ITokenProvider token, ILogger<DesignationRepository> logger)
        : base(factory, token, logger) { }

    public async Task<IEnumerable<DesignationDTO>> GetDesignations()
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync("Designations/getall");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<List<DesignationDTO>>() ?? new();
    }

    public async Task<DesignationDTO?> GetDesignationById(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").GetAsync($"Designations/get/{id}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<DesignationDTO>() : null;
    }

    public async Task<ResponseResult> SaveUpdateDesignation(DesignationDTO designation)
    {
        var content = JsonContent.Create(new { DesignationDTO = designation });
        var res = await GetAuthenticatedClient("MainApi").PostAsync("Designations/insert-update", content);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ResponseResult>() ?? new() { IsSuccessStatus = false };
    }

    public async Task DeleteDesignation(int id)
    {
        var res = await GetAuthenticatedClient("MainApi").DeleteAsync($"Designations/delete/{id}");
        res.EnsureSuccessStatusCode();
    }

}
