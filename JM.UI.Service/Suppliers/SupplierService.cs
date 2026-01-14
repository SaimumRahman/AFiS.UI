using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Suppliers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Suppliers
{
    public class SupplierService : ISupplierService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public SupplierService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SupplierModelDTO>> GetSuppliers()
        {
            try
            {
                return await _unitOfWork.SupplierRepository.GetSuppliers();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SupplierModelDTO?> GetSupplierById(int id)
        {
            try
            {
                return await _unitOfWork.SupplierRepository.GetSupplierById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateSupplier(SupplierModelDTO supplier)
        {
            try
            {
                return await _unitOfWork.SupplierRepository.SaveUpdateSupplier(supplier);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteSupplier(int id)
        {
            try
            {
                await _unitOfWork.SupplierRepository.DeleteSupplier(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Supplier deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
