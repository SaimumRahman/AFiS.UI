using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Shift;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Shift
{
    public class ShiftService : IShiftService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ShiftService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<ShiftDTO>> GetShift()
        {
            var companies = await _repositoryUnitOfWork.ShiftRepository.GetShift();
            return companies.Select(c => new ShiftDTO
            {
                Id = c.Id,
                Name = c.Name,
                ShiftCode = c.ShiftCode,
                Start = c.Start,
                End = c.End,
                CheckEnd = c.CheckEnd,
                CheckEndFinish = c.CheckEndFinish,
                CheckStart = c.CheckStart,
                CheckStartFinish = c.CheckStartFinish,
                DutyType = c.DutyType,
                LastLoginTime = c.LastLoginTime,
                LateCountFrom = c.LateCountFrom,
                LateDeductionDays = c.LateDeductionDays,
                LateDeductionHour = c.LateDeductionHour,
                OvertimeSalaryPercentage = c.OvertimeSalaryPercentage,
                StoreId = c.StoreId,
                TotalHours = c.TotalHours,
                CreatedOn = c.CreatedOn,
                CreatedBy = c.CreatedBy,
                ModifiedOn = c.ModifiedOn,
                ModifiedBy = c.ModifiedBy
            }).ToList();
        }

        public async Task<ShiftDTO?> GetShiftById(int id)
        {
            return await _repositoryUnitOfWork.ShiftRepository.GetShiftById(id);
        }

        public async Task<ResponseResult> SaveUpdateShift(ShiftDTO Shift)
        {
            var validation = await ValidateShift(Shift);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (Shift.Id == 0)
            {
                Shift.CreatedOn = DateTime.Now;
            }
            else
            {
                Shift.ModifiedOn = DateTime.Now;
            }

            return await _repositoryUnitOfWork.ShiftRepository.SaveUpdateShift(Shift);
        }

        public async Task<ResponseResult> DeleteShift(int id)
        {
            try
            {
                await _repositoryUnitOfWork.ShiftRepository.DeleteShift(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Shift deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete Shift: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateShift(ShiftDTO Shift)
        {
            if (string.IsNullOrWhiteSpace(Shift.Name))
                return Task.FromResult((false, "Shift name is required."));

            if (Shift.Name.Length > 100)
                return Task.FromResult((false, "Shift name cannot exceed 100 characters."));

            return Task.FromResult((true, string.Empty));
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public ShiftDTO CreateNewShift()
        {
            return new ShiftDTO
            {
                CreatedOn = DateTime.Now
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
