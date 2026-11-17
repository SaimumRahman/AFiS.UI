using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Discount;
using JM.UI.Entities.Model.SalesPOS;
using System;
using System.Collections.Generic;
using System.Text;

namespace JM.UI.DataService.DAL.Discount
{
    public interface IDiscountManagerRepository
    {
        Task<IEnumerable<DiscountManagerDTO>> GetAll();
        Task<DiscountManagerDTO?> GetById(int id);
        Task<ResponseResult> SaveUpdate(DiscountManagerDTO discountManager);
        Task<ResponseResult> Delete(int id);
        Task<IEnumerable<DiscountTypeDTO>> GetDiscountTypes();

        /// <summary>
        /// Pre-check: products already in another active campaign (same store) whose dates
        /// share a day with this campaign.
        /// </summary>
        Task<IEnumerable<CampaignConflictDTO>> GetConflictsAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> storeIds,
            DateTime startDate,
            DateTime endDate,
            int excludeCampaignId);

        Task<IEnumerable<ProductSearchDTO>> SearchCampaignProductsAsync(
            string term, IEnumerable<int> storeIds);

        Task<IEnumerable<ProductSearchDTO>> SearchCampaignBarcodeAsync(
            string barcode, IEnumerable<int> storeIds);
    }
}
