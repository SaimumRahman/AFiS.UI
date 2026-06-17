using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.MembershipType;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.MembershipType
{
    public class MembershipTypeService : IMembershipTypeService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public MembershipTypeService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<MembershipTypeDTO>> GetAll()
        {
            return await _repositoryUnitOfWork.MembershipTypeRepository.GetAll();
        }

        public async Task<MembershipTypeDTO?> GetById(int id)
        {
            return await _repositoryUnitOfWork.MembershipTypeRepository.GetById(id);
        }

        public async Task<ResponseResult> SaveUpdate(MembershipTypeDTO membershipType)
        {
            var validation = await Validate(membershipType);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            return await _repositoryUnitOfWork.MembershipTypeRepository.SaveUpdate(membershipType);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.MembershipTypeRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete membership type: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> Validate(MembershipTypeDTO membershipType)
        {
            if (string.IsNullOrWhiteSpace(membershipType.Name))
                return Task.FromResult((false, "Membership type name is required."));

            if (membershipType.Name.Length > 100)
                return Task.FromResult((false, "Membership type name cannot exceed 100 characters."));

            if (membershipType.DurationInMonths.HasValue && membershipType.DurationInMonths.Value <= 0)
                return Task.FromResult((false, "Duration in months must be greater than 0."));

            if (membershipType.DiscountRate.HasValue && (membershipType.DiscountRate.Value < 0 || membershipType.DiscountRate.Value > 100))
                return Task.FromResult((false, "Discount rate must be between 0 and 100."));

            return Task.FromResult((true, string.Empty));
        }

        public MembershipTypeDTO CreateNew()
        {
            return new MembershipTypeDTO();
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
