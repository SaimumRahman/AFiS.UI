using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Discount;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Discount
{
    public class DiscountManagerService : IDiscountManagerService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public DiscountManagerService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<DiscountManagerDTO>> GetAll()
        {
            return await _repositoryUnitOfWork.DiscountManagerRepository.GetAll();
        }

        public async Task<DiscountManagerDTO?> GetById(int id)
        {
            return await _repositoryUnitOfWork.DiscountManagerRepository.GetById(id);
        }

        public async Task<ResponseResult> SaveUpdate(DiscountManagerDTO discountManager)
        {
            var validation = await Validate(discountManager);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (discountManager.Id == 0)
            {
                discountManager.CreatedDate = DateTime.Now;
            }
            else
            {
                discountManager.ModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork.DiscountManagerRepository.SaveUpdate(discountManager);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.DiscountManagerRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete discount campaign: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<DiscountTypeDTO>> GetDiscountTypes()
        {
            return await _repositoryUnitOfWork.DiscountManagerRepository.GetDiscountTypes();
        }

        public Task<(bool IsValid, string ErrorMessage)> Validate(DiscountManagerDTO discountManager)
        {
            if (string.IsNullOrWhiteSpace(discountManager.DiscountName))
                return Task.FromResult((false, "Campaign name is required."));

            if (discountManager.DiscountName.Length > 200)
                return Task.FromResult((false, "Campaign name cannot exceed 200 characters."));

            if (discountManager.StartDate == default)
                return Task.FromResult((false, "Start date is required."));

            if (discountManager.EndDate == default)
                return Task.FromResult((false, "End date is required."));

            if (discountManager.EndDate <= discountManager.StartDate)
                return Task.FromResult((false, "End date must be after start date."));

            if (discountManager.DiscountDetails == null || discountManager.DiscountDetails.Count == 0)
                return Task.FromResult((false, "At least one product must be selected with a discount."));

            foreach (var detail in discountManager.DiscountDetails)
            {
                if (detail.DiscountValue <= 0)
                    return Task.FromResult((false, $"Discount value must be greater than 0 for item '{detail.ItemName}'."));

                if (detail.DiscountTypeId == 1 && detail.DiscountValue > 100)
                    return Task.FromResult((false, $"Percentage discount cannot exceed 100% for item '{detail.ItemName}'."));
            }

            return Task.FromResult((true, string.Empty));
        }

        public DiscountManagerDTO CreateNew()
        {
            return new DiscountManagerDTO
            {
                IsActive = true,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(1),
                CreatedDate = DateTime.Now
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
