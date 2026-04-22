using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.Service.Transfer
{
    public interface ITransferService
    {
        Task<IEnumerable<TransferMasterDTO>> GetTransfers();
        Task<TransferMasterDTO?> GetTransferById(long id);
        Task<ResponseResult> SaveUpdateTransfer(TransferMasterDTO transfer);
        Task<ResponseResult> DeleteTransfer(long id, int deletedBy);
        Task<ResponseResult> DeleteTransferDetail(long detailId, int deletedBy);
        Task<(bool IsValid, string ErrorMessage)> ValidateTransfer(TransferMasterDTO transfer);
        TransferMasterDTO CreateNewTransfer(int companyId, int createdBy);
        TransferDetailDTO CreateNewDetailLine();
        string GetCompletedBadgeStyle(int isCompleted);
        string GetReceiveStatusBadgeStyle(int statusId);
        string Truncate(string? value, int maxChars);
        Task<ItemDTO?> SearchByBarcodeExact(string barcode, int storeId);
        Task<IEnumerable<ItemDTO?>> SearchByBarcodeUptoColor(string barcode, int storeId);
        Task<IEnumerable<TransferDetailDTO>> GetDetailsByTransferId(long transferId);
        Task<IEnumerable<TransferMasterDTO>> GetUndispatchedTransfers(int storeId);
        Task<IEnumerable<TransferMasterDTO>> GetDispatchedTransfers(int storeId);
        Task<ResponseResult> UpdateDispatchStatus(List<int> transferIds, int updatedBy);
        Task<ResponseResult> UpdateReceivedStatus(List<int> transferIds, int updatedBy);
    }
}
