using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.InvRequisition;

namespace JM.UI.Service.InvRequisition
{
    public class InvRequisitionService : IInvRequisitionService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public InvRequisitionService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<InvRequisitionMasterDTO>> GetAll()
        {
            return await _repositoryUnitOfWork.InvRequisitionRepository.GetAll();
        }

        public async Task<IEnumerable<InvRequisitionMasterDTO>> GetAllByStoreId(int storeId)
        {
            return await _repositoryUnitOfWork.InvRequisitionRepository.GetAllByStoreId(storeId);
        }

        public async Task<InvRequisitionMasterDTO?> GetById(int id)
        {
            return await _repositoryUnitOfWork.InvRequisitionRepository.GetById(id);
        }

        public async Task<ResponseResult> SaveUpdate(InvRequisitionMasterDTO requisition)
        {
            var validation = await Validate(requisition);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (requisition.RequisitionID == 0)
            {
                requisition.CreateOn = DateTime.Now;
            }
            else
            {
                requisition.UpdateOn = DateTime.Now;
            }

            return await _repositoryUnitOfWork.InvRequisitionRepository.InsertUpdate(requisition);
        }

        public async Task<ResponseResult> Delete(int id)
        {
            try
            {
                return await _repositoryUnitOfWork.InvRequisitionRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete requisition: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<RequisitionStatusDTO>> GetRequisitionStatuses()
        {
            return await _repositoryUnitOfWork.InvRequisitionRepository.GetRequisitionStatuses();
        }
        public async Task<ResponseResult> UpdateRequisitionStatus(int RequisitionId, int StatusId, int StatusBy, string StatusComments)
            => await _repositoryUnitOfWork.InvRequisitionRepository.UpdateRequisitionStatus(RequisitionId, StatusId, StatusBy, StatusComments);
        
        public Task<(bool IsValid, string ErrorMessage)> Validate(InvRequisitionMasterDTO requisition)
        {
            if (requisition.RequisitionDate == default)
                return Task.FromResult((false, "Requisition date is required."));

            if (requisition.Details == null || !requisition.Details.Any())
                return Task.FromResult((false, "At least one item is required."));

            foreach (var detail in requisition.Details)
            {
                if (detail.ItemID <= 0)
                    return Task.FromResult((false, "A valid item must be selected for all detail lines."));

                if (detail.Qty <= 0)
                    return Task.FromResult((false, $"Quantity must be greater than 0 for item '{detail.ItemName ?? detail.ItemID.ToString()}'."));
            }

            return Task.FromResult((true, string.Empty));
        }

        public InvRequisitionMasterDTO CreateNew()
        {
            return new InvRequisitionMasterDTO
            {
                RequisitionDate = DateTime.Today,
                CreateOn = DateTime.Now,
                Details = new List<InvRequisitionDetailDTO>()
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }
    }
}
