using System;
using System.Collections.Generic;
using System.Text;
using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Designations;

namespace JM.UI.Service.Designations;

public class DesignationService : IDesignationService
{
    private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

    public DesignationService(IRepositoryUnitOfWork repositoryUnitOfWork)
        => _repositoryUnitOfWork = repositoryUnitOfWork;

    public async Task<IEnumerable<DesignationDTO>> GetDesignations()
        => await _repositoryUnitOfWork.DesignationRepository.GetDesignations();

    public async Task<DesignationDTO?> GetDesignationById(int id)
        => await _repositoryUnitOfWork.DesignationRepository.GetDesignationById(id);

    public async Task<ResponseResult> SaveUpdateDesignation(DesignationDTO designation)
    {
        var v = await ValidateDesignation(designation);
        if (!v.IsValid)
            return new() { IsSuccessStatus = false, Message = v.ErrorMessage };

        return await _repositoryUnitOfWork.DesignationRepository.SaveUpdateDesignation(designation);
    }

    public async Task<ResponseResult> DeleteDesignation(int id)
    {
        try
        {
            await _repositoryUnitOfWork.DesignationRepository.DeleteDesignation(id);
            return new() { IsSuccessStatus = true, Message = "Designation deleted" };
        }
        catch (Exception ex)
        {
            return new() { IsSuccessStatus = false, Message = ex.Message };
        }
    }

    public Task<(bool IsValid, string ErrorMessage)> ValidateDesignation(DesignationDTO d)
    {
        if (string.IsNullOrWhiteSpace(d.Name))
            return Task.FromResult((false, "Designation Name is required"));

        return Task.FromResult((true, string.Empty));
    }


    public string Truncate(string? value, int maxChars)
        => value?.Length > maxChars ? value[..maxChars] + "..." : value ?? "";
}