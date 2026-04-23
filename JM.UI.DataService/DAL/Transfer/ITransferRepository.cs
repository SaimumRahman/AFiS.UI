using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Items;
using JM.UI.Entities.Model.Transfer;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Transfer
{
    public interface ITransferRepository
    {
        Task<IEnumerable<TransferMasterDTO>> GetTransfers();
        Task<TransferMasterDTO?> GetTransferById(long id);
        Task<ResponseResult> SaveUpdateTransfer(TransferMasterDTO transfer);
        Task<ResponseResult> DeleteTransfer(long id, int deletedBy);
        Task<ResponseResult> DeleteTransferDetail(long detailId, int deletedBy);
        Task<ItemDTO?> SearchByBarcodeExact(string barcode, int storeId);
        Task<IEnumerable<ItemDTO?>> SearchByBarcodeUptoColor(string barcode, int storeId);
        Task<IEnumerable<TransferDetailDTO>> GetDetailsByTransferId(long transferId);
        Task<IEnumerable<TransferMasterDTO>> GetUndispatchedTransfers(int storeId);
        Task<IEnumerable<TransferMasterDTO>> GetDispatchedTransfers(int storeId);
        Task<ResponseResult> UpdateDispatchStatus(List<int> transferIds, int updatedBy);
        Task UpdateReceivedStatus(List<int> receivedDetailIds, List<int> fullyReceivedMasterIds, DateTime now, int userId);
    }
}
