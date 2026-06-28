using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Items;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items
{
    public class TransferTypeService : ITransferTypeService
    {
        private readonly IRepositoryUnitOfWork _unitOfWork;

        public TransferTypeService(IRepositoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TransferTypeDTO>> GetTransferTypes()
        {
            try
            {
                return await _unitOfWork.TransferTypeRepository.GetTransferTypes();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
