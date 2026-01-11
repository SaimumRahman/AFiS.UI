using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Colors;
using JM.UI.Service.Colors;

namespace JM.UI.Service.Colors;

public class ColorsService : IColorsService
{
    private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

    public ColorsService(IRepositoryUnitOfWork repositoryUnitOfWork)
        => _repositoryUnitOfWork = repositoryUnitOfWork;

    public async Task<IEnumerable<ColorsDTO>> GetColorss()
        => await _repositoryUnitOfWork.ColorsRepository.GetColors();

    public async Task<ColorsDTO?> GetColorsById(int id)
        => await _repositoryUnitOfWork.ColorsRepository.GetColorsById(id);

    public async Task<ResponseResult> SaveUpdateColors(ColorsDTO Colors)
    {
        var v = await ValidateColors(Colors);
        if (!v.IsValid)
            return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

        return await _repositoryUnitOfWork.ColorsRepository.SaveUpdateColors(Colors);
    }

    public async Task<ResponseResult> DeleteColors(int id)
    {
        try
        {
            await _repositoryUnitOfWork.ColorsRepository.DeleteColors(id);
            return new() { IsSuccessStatus = true, Message = "Colors deleted" };
        }
        catch (Exception ex)
        {
            return new() { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateColors(ColorsDTO d)
    {
        if (string.IsNullOrWhiteSpace(d.Name))
            return Task.FromResult((false, "Colors Name is required"));

        return Task.FromResult((true, string.Empty));
    }


    public string Truncate(string? value, int maxChars)
        => value?.Length > maxChars ? value[..maxChars] + "..." : value ?? "";
}