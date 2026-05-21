using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Transfer
{
    public class TransferService : ITransferService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public TransferService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<ResponseResult> UpdateDispatchStatus(List<int> transferIds, int updatedBy)
            => await _repositoryUnitOfWork.TransferRepository.UpdateDispatchStatus(transferIds, updatedBy);
        public async Task UpdateReceivedStatus(List<int> receivedDetailIds, List<int> fullyReceivedMasterIds, DateTime now, int userId)
            => await _repositoryUnitOfWork.TransferRepository.UpdateReceivedStatus(receivedDetailIds, fullyReceivedMasterIds, now, userId);

        public async Task<IEnumerable<TransferMasterDTO>> GetTransfers()
            => await _repositoryUnitOfWork.TransferRepository.GetTransfers();
        public async Task<IEnumerable<TransferMasterDTO>> GetUndispatchedTransfers(int storeId)
            => await _repositoryUnitOfWork.TransferRepository.GetUndispatchedTransfers(storeId);
        public async Task<IEnumerable<TransferMasterDTO>> GetDispatchedTransfers(int storeId)
            => await _repositoryUnitOfWork.TransferRepository.GetDispatchedTransfers(storeId);

        public async Task<TransferMasterDTO?> GetTransferById(long id)
            => await _repositoryUnitOfWork.TransferRepository.GetTransferById(id);

        public async Task<IEnumerable<TransferDetailDTO>> GetDetailsByTransferId(long transferId)
            => await _repositoryUnitOfWork.TransferRepository.GetDetailsByTransferId(transferId);

        public async Task<ResponseResult> SaveUpdateTransfer(TransferMasterDTO transfer)
        {
            var validation = await ValidateTransfer(transfer);
            if (!validation.IsValid)
                return new ResponseResult { IsSuccessStatus = false, Message = validation.ErrorMessage };

            transfer.CreatedAt = DateTime.Now;

            return await _repositoryUnitOfWork.TransferRepository.SaveUpdateTransfer(transfer);
        }

        public async Task<ResponseResult> DeleteTransfer(long id, int deletedBy)
        {
            try
            {
                return await _repositoryUnitOfWork.TransferRepository.DeleteTransfer(id, deletedBy);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = $"Failed to delete transfer: {ex.Message}" };
            }
        }

        public async Task<ResponseResult> DeleteTransferDetail(long detailId, int deletedBy)
        {
            try
            {
                return await _repositoryUnitOfWork.TransferRepository.DeleteTransferDetail(detailId, deletedBy);
            }
            catch (Exception ex)
            {
                return new ResponseResult { IsSuccessStatus = false, Message = $"Failed to delete detail line: {ex.Message}" };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateTransfer(TransferMasterDTO transfer)
        {
            if (transfer.StoreId <= 0)
                return Task.FromResult((false, "Please select a valid source store."));

            if (transfer.TransTypeID <= 0)
                return Task.FromResult((false, "Transfer type is required."));

            if (transfer.TransferDate == default)
                return Task.FromResult((false, "Transfer date is required."));

            if (transfer.CompanyID <= 0)
                return Task.FromResult((false, "Company is required."));

            if (transfer.Details == null || !transfer.Details.Any())
                return Task.FromResult((false, "At least one item detail is required."));

            foreach (var detail in transfer.Details)
            {
                if (detail.ItemID <= 0)
                    return Task.FromResult((false, "A valid item must be selected for all detail lines."));

                if (detail.IssueQty <= 0)
                    return Task.FromResult((false, $"Issue quantity must be greater than 0 for item: {detail.ItemName ?? detail.ItemID.ToString()}."));

                if (detail.UnitID <= 0)
                    return Task.FromResult((false, $"A valid unit is required for item: {detail.ItemName ?? detail.ItemID.ToString()}."));
            }

            return Task.FromResult((true, string.Empty));
        }

        public TransferMasterDTO CreateNewTransfer(int companyId, int createdBy)
        {
            return new TransferMasterDTO
            {
                TransferDate = DateTime.Now,
                IsCompleted = 0,
                ReceiveStatusId = 1,
                CompanyID = companyId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now,
                Details = new List<TransferDetailDTO> { CreateNewDetailLine() }
            };
        }

        public TransferDetailDTO CreateNewDetailLine()
        {
            return new TransferDetailDTO
            {
                IssueQty = 1,
                UnitPrice = 0,
                TotalAmount = 0
            };
        }

        public string GetCompletedBadgeStyle(int isCompleted)
        {
            return isCompleted == 1
                ? "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;"
                : "background-color: #ff9800; color: white; padding: 4px 8px; border-radius: 4px;";
        }

        public string GetReceiveStatusBadgeStyle(int statusId)
        {
            return statusId switch
            {
                1 => "background-color: #9e9e9e; color: white; padding: 4px 8px; border-radius: 4px;", // Pending
                2 => "background-color: #2196f3; color: white; padding: 4px 8px; border-radius: 4px;", // Partially Received
                3 => "background-color: #4caf50; color: white; padding: 4px 8px; border-radius: 4px;", // Fully Received
                _ => "background-color: #f44336; color: white; padding: 4px 8px; border-radius: 4px;"  // Unknown
            };
        }

        public string Truncate(string? value, int maxChars)
        {
            return value?.Length > maxChars ? value.Substring(0, maxChars) + "..." : value ?? string.Empty;
        }

        public Task<ItemDTO?> SearchByBarcodeExact(string barcode, int storeId)
        => _repositoryUnitOfWork.TransferRepository.SearchByBarcodeExact(barcode,storeId);
        public Task<IEnumerable<ItemDTO?>> SearchByBarcodeUptoColor(string barcode, int storeId)
        => _repositoryUnitOfWork.TransferRepository.SearchByBarcodeUptoColor(barcode,storeId);
    }
}
