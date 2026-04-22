using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Designs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Designs
{
    public class DesignService : IDesignService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public DesignService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<DesignModelDTO>> GetDesigns()
        {
            try
            {
                return await _unitOfWork.DesignRepository.GetDesigns();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<DesignModelDTO> GetDesignCode()
        {
            try
            {
                return await _unitOfWork.DesignRepository.GetDesignCode();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DesignModelDTO?> GetDesignById(int id)
        {
            try
            {
                return await _unitOfWork.DesignRepository.GetDesignById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DesignModelDTO>> LoadDesignsBySubGroup(int subGroupId)
        {
            try
            {
                return await _unitOfWork.DesignRepository.LoadDesignsBySubGroup(subGroupId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateDesign(DesignModelDTO design)
        {
            try
            {
                return await _unitOfWork.DesignRepository.SaveUpdateDesign(design);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteDesign(int id)
        {
            try
            {
                await _unitOfWork.DesignRepository.DeleteDesign(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Design deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
