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
    }
}
