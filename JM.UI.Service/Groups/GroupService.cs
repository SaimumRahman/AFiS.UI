using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Groups;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Groups
{
    public class GroupService : IGroupService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public GroupService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<GroupModelDTO>> GetGroups()
        {
            try
            {
                return await _unitOfWork.GroupRepository.GetGroups();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<GroupModelDTO?> GetGroupById(int id)
        {
            try
            {
                return await _unitOfWork.GroupRepository.GetGroupById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateGroup(GroupModelDTO group)
        {
            try
            {
                return await _unitOfWork.GroupRepository.SaveUpdateGroup(group);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteGroup(int id)
        {
            try
            {
                await _unitOfWork.GroupRepository.DeleteGroup(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Group deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
