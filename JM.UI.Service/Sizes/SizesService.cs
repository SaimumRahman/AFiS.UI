using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Sizes;


namespace JM.UI.Service.Sizes;

public class Sizeservice : ISizesService
{
    private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

    public Sizeservice(IRepositoryUnitOfWork repositoryUnitOfWork)
        => _repositoryUnitOfWork = repositoryUnitOfWork;

    
    public async Task<IEnumerable<SizesDTO>> GetSizess()
        => await _repositoryUnitOfWork.SizesRepository.GetSizess();

    
    public async Task<SizesDTO?> GetSizesById(int id)
        => await _repositoryUnitOfWork.SizesRepository.GetSizesById(id);

    public async Task<ResponseResult> SaveUpdateSizes(SizesDTO Sizes)
    {
        var v = await ValidateSizes(Sizes);
        if (!v.IsValid)
            return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

        return await _repositoryUnitOfWork.SizesRepository.SaveUpdateSizes(Sizes);
    }

    public async Task<ResponseResult> DeleteSizes(int id)
    {
        try
        {
            await _repositoryUnitOfWork.SizesRepository.DeleteSizes(id);
            return new() { IsSuccessStatus = true, Message = "Sizes deleted" };
        }
        catch (Exception ex)
        {
            return new() { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateSizes(SizesDTO d)
    {
        if (string.IsNullOrWhiteSpace(d.Name))
            return Task.FromResult((false, "Sizes Name is required"));

        return Task.FromResult((true, string.Empty));
    }


    public string Truncate(string? value, int maxChars)
        => value?.Length > maxChars ? value[..maxChars] + "..." : value ?? "";
}