using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.SubGroups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.SubGroups
{
    public class SubGroupService : ISubGroupService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public SubGroupService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SubGroupModelDTO>> GetSubGroups()
        {
            try
            {
                return await _unitOfWork.SubGroupRepository.GetSubGroups();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SubGroupModelDTO?> GetSubGroupById(int id)
        {
            try
            {
                return await _unitOfWork.SubGroupRepository.GetSubGroupById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<IEnumerable<SubGroupModelDTO>> LoadSubGroupsByGroup(int groupId)
        {
            try
            {
                return await _unitOfWork.SubGroupRepository.LoadSubGroupsByGroup(groupId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSubGroup(SubGroupModelDTO subGroup)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(subGroup.Code))
                {
                    return new ResponseResult("Code is required.", subGroup.Id);
                }
                if (subGroup.Code.Length != 3)
                {
                    return new ResponseResult("Code must be exactly 3 characters.", subGroup.Id);
                }
                var exists = await _unitOfWork.SubGroupRepository.IsCodeExistsAsync(subGroup.Code, subGroup.Id);
                if (exists)
                {
                    return new ResponseResult("Code already exists.", subGroup.Id);
                }
                return await _unitOfWork.SubGroupRepository.SaveUpdateSubGroup(subGroup);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteSubGroup(int id)
        {
            try
            {
                await _unitOfWork.SubGroupRepository.DeleteSubGroup(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "SubGroup deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<bool> IsCodeExists(string code, int id = 0)
        {
            return await _unitOfWork.SubGroupRepository.IsCodeExistsAsync(code, id);
        }
    }
}
