using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Bank;
using JM.UI.Service.Banks;

namespace JM.UI.Service.Bankss;

public class BanksService : IBanksService
{
    private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

    public BanksService(IRepositoryUnitOfWork repositoryUnitOfWork)
        => _repositoryUnitOfWork = repositoryUnitOfWork;

    public async Task<IEnumerable<BanksDTO>> GetBankss()
        => await _repositoryUnitOfWork.BanksRepository.GetBankss();

    public async Task<BanksDTO?> GetBanksById(int id)
        => await _repositoryUnitOfWork.BanksRepository.GetBanksById(id);

    public async Task<ResponseResult> SaveUpdateBanks(BanksDTO Banks)
    {
        var v = await ValidateBanks(Banks);
        if (!v.IsValid)
            return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

        return await _repositoryUnitOfWork.BanksRepository.SaveUpdateBanks(Banks);
    }

    public async Task<ResponseResult> DeleteBanks(int id)
    {
        try
        {
            await _repositoryUnitOfWork.BanksRepository.DeleteBanks(id);
            return new() { IsSuccessStatus = true, Message = "Banks deleted" };
        }
        catch (Exception ex)
        {
            return new() { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateBanks(BanksDTO d)
    {
        if (string.IsNullOrWhiteSpace(d.Name))
            return Task.FromResult((false, "Banks Name is required"));

        return Task.FromResult((true, string.Empty));
    }


    public string Truncate(string? value, int maxChars)
        => value?.Length > maxChars ? value[..maxChars] + "..." : value ?? "";
}