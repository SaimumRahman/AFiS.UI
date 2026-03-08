using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JM.UI.Service.Purchases
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public PurchaseService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        // =============================================
        // Get All Purchases
        // =============================================
        public async Task<IEnumerable<PurchaseSummaryDTO>> GetAllPurchases()
        {
            return await _repositoryUnitOfWork.PurchaseRepository.GetPurchases();
        }

        // =============================================
        // Get Purchase By Id
        // =============================================
        public async Task<PurchaseDTO?> GetPurchaseById(int id)
        {
            return await _repositoryUnitOfWork.PurchaseRepository.GetPurchaseById(id);
        }

        // =============================================
        // Save/Update Purchase
        // =============================================
        public async Task<ResponseResult> SaveUpdatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items)
        {
            var validation = await ValidatePurchase(purchase, items);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            // Calculate totals before sending
            purchase.TotalAmount = CalculatePurchaseTotal(items);
            purchase.NetAmount = purchase.TotalAmount - (purchase.DiscountAmount ?? 0) + (purchase.VatAmount ?? 0);
            purchase.DueAmount = purchase.NetAmount - (purchase.PaidAmount ?? 0);

            if (purchase.Id == 0)
                purchase.CreatedDate = DateTime.Now;
            else
                purchase.LastModifiedDate = DateTime.Now;

            // All ItemWiseFeature logic now lives inside the API handler.
            // The result carries SavedItems back if the UI needs them for display,
            // but no further action is required here.
            return await _repositoryUnitOfWork.PurchaseRepository.SaveUpdatePurchase(purchase, items);
        }
        public async Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItems(int purchaseId)
        {
            try
            {
                var response = await _repositoryUnitOfWork.PurchaseRepository.GetPurchaseItems(purchaseId);
                return response ?? new List<PurchaseItemDTO>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        // =============================================
        // Delete Purchase
        // =============================================
        public async Task<ResponseResult> DeletePurchase(int id)
        {
            try
            {
                await _repositoryUnitOfWork.PurchaseRepository.DeletePurchase(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Purchase deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete purchase: {ex.Message}"
                };
            }
        }

        // =============================================
        // Generate Barcode
        // =============================================
        public async Task<string> GenerateBarcode(BarcodeGenerationRequestDTO request)
        {
            return await _repositoryUnitOfWork.PurchaseRepository.GenerateBarcode(request);
        }

        // =============================================
        // Search By Barcode
        // =============================================
        public async Task<BarcodeSearchResponseDTO> SearchByBarcode(string barcode)
        {
            return await _repositoryUnitOfWork.PurchaseRepository.SearchByBarcode(barcode);
        }

        // =============================================
        // Validate Purchase
        // =============================================
        public Task<(bool IsValid, string ErrorMessage)> ValidatePurchase(PurchaseDTO purchase, List<PurchaseItemDTO> items)
        {
            if (purchase.SupplierId == null || purchase.SupplierId <= 0)
                return Task.FromResult((false, "Supplier is required."));

            if (purchase.PurchaseDate == default)
                return Task.FromResult((false, "Purchase date is required."));

            if (purchase.StoreId == null || purchase.StoreId <= 0)
                return Task.FromResult((false, "Store is required."));

            if (items == null || !items.Any())
                return Task.FromResult((false, "At least one item is required."));

            foreach (var item in items)
            {

                if (item.Quantity <= 0)
                    return Task.FromResult((false, "Quantity must be greater than 0."));

                if (item.PurchasePrice <= 0)
                    return Task.FromResult((false, "Purchase price must be greater than 0."));

                if (item.IsSaleable && (!item.SalePrice.HasValue || item.SalePrice.Value <= 0))
                    return Task.FromResult((false, $"Item '{item.ItemName}' is marked as saleable but has no sale price."));

                if (item.IsSaleable && item.SalePrice.HasValue && item.SalePrice.Value <= item.PurchasePrice)
                    return Task.FromResult((false, $"Sale price for '{item.ItemName}' must be greater than purchase price."));

                if (string.IsNullOrWhiteSpace(item.Barcode))
                    return Task.FromResult((false, $"Barcode is required for item '{item.ItemName}'."));
            }

            return Task.FromResult((true, string.Empty));
        }

        // =============================================
        // Create New Purchase
        // =============================================
        public PurchaseDTO CreateNewPurchase()
        {
            return new PurchaseDTO
            {
                PurchaseDate = DateTime.Now,
                IsActive = true,
                IsVatIncluded = false,
                PurchaseItems = new List<PurchaseItemDTO>()
            };
        }

        // =============================================
        // Calculate Item Total
        // =============================================
        public decimal CalculateItemTotal(PurchaseItemDTO item)
        {
            decimal baseAmount = item.Quantity * item.PurchasePrice;
            decimal otherCosts = (item.OtherCost ?? 0) + (item.CarryingCost ?? 0) + (item.OperationalCost ?? 0);
            decimal subtotal = baseAmount + otherCosts;

            // VAT Calculation
            if (item.VatPercentage.HasValue && item.VatPercentage.Value > 0)
            {
                item.VatAmount = subtotal * (item.VatPercentage.Value / 100);
                subtotal += item.VatAmount.Value;
            }

            item.TotalAmount = subtotal;
            return item.TotalAmount;
        }

        // =============================================
        // Calculate Purchase Total
        // =============================================
        public decimal CalculatePurchaseTotal(List<PurchaseItemDTO> items)
        {
            return items.Sum(x => x.TotalAmount);
        }
        public async Task<IEnumerable<PurchaseDraftDTO>> GetPurchaseDrafts()
        {
            return await _repositoryUnitOfWork.PurchaseRepository.GetPurchaseDrafts();
        }

        public async Task<PurchaseDraftDTO?> GetPurchaseDraftById(int id)
        {
            return await _repositoryUnitOfWork.PurchaseRepository.GetPurchaseDraftById(id);
        }

        public async Task<ResponseResult> SavePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items)
        {
            var validation = await ValidatePurchaseDraft(draft, items);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (draft.Id == 0)
            {
                draft.CreatedDate = DateTime.Now;
            }
            else
            {
                draft.LastModifiedDate = DateTime.Now;
            }

            return await _repositoryUnitOfWork.PurchaseRepository.SavePurchaseDraft(draft, items);
        }

        public async Task<ResponseResult> DeletePurchaseDraft(int id)
        {
            try
            {
                await _repositoryUnitOfWork.PurchaseRepository.DeletePurchaseDraft(id);
                return new ResponseResult
                {
                    IsSuccessStatus = true,
                    Message = "Draft deleted successfully!"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = $"Failed to delete draft: {ex.Message}"
                };
            }
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidatePurchaseDraft(PurchaseDraftDTO draft, List<PurchaseDraftItemDTO> items)
        {
            if (string.IsNullOrWhiteSpace(draft.DraftName))
                return Task.FromResult((false, "Draft name is required."));

            if (draft.DraftName.Length > 200)
                return Task.FromResult((false, "Draft name cannot exceed 200 characters."));

            if (items == null || items.Count == 0)
                return Task.FromResult((false, "At least one item is required to save draft."));

            return Task.FromResult((true, string.Empty));
        }

        public string FormatCurrency(decimal amount)
        {
            return amount.ToString("N2");
        }

        public string FormatDate(DateTime? date)
        {
            return date?.ToString("dd/MM/yyyy HH:mm") ?? "-";
        }
    }
}
