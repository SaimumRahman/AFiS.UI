using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.CostCenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Service.CostCenters
{
    public class CostCenterService : ICostCenterService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public CostCenterService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<CostCenterDTO>> GetCostCenters(bool? onlyActive = null)
        {
            var costCenters = await _repositoryUnitOfWork.CostCenterRepository.GetCostCenters(onlyActive);

            if (onlyActive == true)
            {
                costCenters = costCenters.Where(c => c.IsActive);
            }

            return costCenters.ToList();
        }

        public async Task<CostCenterDTO?> GetCostCenterById(int id)
        {
            return await _repositoryUnitOfWork.CostCenterRepository.GetCostCenterById(id);
        }

        public async Task<ResponseResult> SaveUpdateCostCenter(CostCenterDTO costCenter)
        {
            var validation = await ValidateCostCenter(costCenter);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            return await _repositoryUnitOfWork.CostCenterRepository.SaveUpdateCostCenter(costCenter);
        }

        public async Task<ResponseResult> DeleteCostCenter(int id)
        {
            try
            {
                await _repositoryUnitOfWork.CostCenterRepository.DeleteCostCenter(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Cost Center deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete cost center: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateCostCenter(CostCenterDTO costCenter)
        {
            if (string.IsNullOrWhiteSpace(costCenter.Code))
                return Task.FromResult((false, "Code is required."));

            if (costCenter.Code.Length > 10)
                return Task.FromResult((false, "Code cannot exceed 10 characters."));

            if (string.IsNullOrWhiteSpace(costCenter.Name))
                return Task.FromResult((false, "Name is required."));

            if (costCenter.Name.Length > 100)
                return Task.FromResult((false, "Name cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        public CostCenterDTO CreateNewCostCenter()
        {
            return new CostCenterDTO
            {
                CompanyId = 1,
                IsActive = true
            };
        }
    }
}