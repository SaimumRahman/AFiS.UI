using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Barcodes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Barcodes
{
    public class BarcodeService : IBarcodeService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public BarcodeService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<BarcodeModelDTO>> GetBarcodes()
        {
            try
            {
                return await _unitOfWork.BarcodeRepository.GetBarcodes();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BarcodeModelDTO?> GetBarcodeById(int id)
        {
            try
            {
                return await _unitOfWork.BarcodeRepository.GetBarcodeById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseResult> SaveUpdateBarcode(BarcodeModelDTO barcode)
        {
            try
            {
                return await _unitOfWork.BarcodeRepository.SaveUpdateBarcode(barcode);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }

        public async Task<ResponseResult> DeleteBarcode(int id)
        {
            try
            {
                await _unitOfWork.BarcodeRepository.DeleteBarcode(id);
                return new ResponseResult { IsSuccessStatus = true, Message = "Barcode Deleted Successfully" };
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = ex.Message };
            }
        }
    }
}
