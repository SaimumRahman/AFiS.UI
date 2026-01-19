using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.MesurementUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.MesurementUnits
{
    public class MesurementUnitService : IMesurementUnitService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public MesurementUnitService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MesurementUnitModelDTO>> GetMesurementUnits()
        {
            try
            {
                return await _unitOfWork.MesurementUnitRepository.GetMesurementUnits();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MesurementUnitModelDTO?> GetMesurementUnitById(int id)
        {
            try
            {
                return await _unitOfWork.MesurementUnitRepository.GetMesurementUnitById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateMesurementUnit(MesurementUnitModelDTO unit)
        {
            try
            {
                return await _unitOfWork.MesurementUnitRepository.SaveUpdateMesurementUnit(unit);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteMesurementUnit(int id)
        {
            try
            {
                await _unitOfWork.MesurementUnitRepository.DeleteMesurementUnit(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Measurement Unit deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
