using JM.Infrastructure.Models;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.SalesPOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Service.SalesPOS
{
    public class SaleService : ISaleService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public SaleService(IRepositoryUnitOfWork repositoryUnitOfWork)
            => _repositoryUnitOfWork = repositoryUnitOfWork;

        public async Task<IEnumerable<SaleSummaryDTO>> GetAllSales()
        {
            return await _repositoryUnitOfWork.SaleRepository.GetSales();
        }

        public async Task<SaleMasterDTO?> GetSaleById(int id)
        {
            return await _repositoryUnitOfWork.SaleRepository.GetSaleById(id);
        }

        public async Task<ResponseResult> SaveSale(SaleMasterDTO sale)
        {
            var validation = await ValidateSale(sale);
            if (!validation.IsValid)
            {
                return new ResponseResult
                {
                    IsSuccessStatus = false,
                    Message = validation.ErrorMessage
                };
            }

            if (string.IsNullOrEmpty(sale.InvoiceNo))
                sale.InvoiceNo = await GetNewInvoiceNo();

            sale.NetAmount = CalculateNetAmount(sale);
            sale.DueAmount = sale.NetAmount - (sale.PaidAmount ?? 0);
            sale.PaymentStatus = sale.DueAmount <= 0 ? "Paid" :
                (sale.PaidAmount > 0 ? "Partial" : "Due");

            return await _repositoryUnitOfWork.SaleRepository.SaveSale(sale);
        }

        public async Task<ResponseResult> DeleteSale(int id)
        {
            return await _repositoryUnitOfWork.SaleRepository.DeleteSale(id);
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetSalesByDateRange(DateTime fromDate, DateTime toDate)
        {
            return await _repositoryUnitOfWork.SaleRepository.GetSalesByDateRange(fromDate, toDate);
        }

        public async Task<IEnumerable<SaleSummaryDTO>> GetSalesByCustomerId(int customerId)
        {
            return await _repositoryUnitOfWork.SaleRepository.GetSalesByCustomerId(customerId);
        }

        public async Task<SaleMasterDTO?> GetSaleByInvoiceNo(string invoiceNo)
        {
            return await _repositoryUnitOfWork.SaleRepository.GetSaleByInvoiceNo(invoiceNo);
        }

        public async Task<string> GetNewInvoiceNo()
        {
            return await _repositoryUnitOfWork.SaleRepository.GetNewInvoiceNo();
        }

        public async Task<IEnumerable<ProductSearchDTO>> SearchProducts(string searchTerm)
        {
            return await _repositoryUnitOfWork.SaleRepository.SearchProducts(searchTerm);
        }

        public async Task<ProductSearchDTO?> SearchByBarcode(string barcode)
        {
            return await _repositoryUnitOfWork.SaleRepository.SearchByBarcode(barcode);
        }

        public Task<(bool IsValid, string ErrorMessage)> ValidateSale(SaleMasterDTO sale)
        {
            if (sale.SaleDetails == null || sale.SaleDetails.Count == 0)
                return Task.FromResult((false, "At least one item is required."));

            if (sale.SaleDetails.Any(d => d.Quantity <= 0))
                return Task.FromResult((false, "Item quantity must be greater than zero."));

            if (sale.SaleDetails.Any(d => d.SalePrice <= 0))
                return Task.FromResult((false, "Item sale price must be greater than zero."));

            return Task.FromResult((true, string.Empty));
        }

        public SaleMasterDTO CreateNew()
        {
            return new SaleMasterDTO
            {
                SalesDate = DateTime.Now,
                SalesType = "Sale",
                PaymentStatus = "Due",
                VatPercentage = 5m,
                InvoiceDiscountType = "Percentage",
                IsActive = true
            };
        }

        public decimal CalculateSubTotal(List<SaleDetailDTO> details)
        {
            return details.Sum(d => d.TotalPrice);
        }

        public decimal CalculateVat(decimal subTotal, decimal vatPercentage)
        {
            return subTotal * (vatPercentage / 100m);
        }

        public decimal CalculateNetAmount(SaleMasterDTO sale)
        {
            decimal net = sale.SubTotal;

            if (sale.InvoiceDiscountType == "Percentage" && sale.InvoiceDiscount.HasValue)
                net -= net * (sale.InvoiceDiscount.Value / 100m);
            else if (sale.InvoiceDiscount.HasValue)
                net -= sale.InvoiceDiscount.Value;

            net -= sale.CampaignDiscount ?? 0;
            net -= sale.MembershipDiscount ?? 0;
            net -= sale.ExchangeAmount ?? 0;
            net += sale.VatAmount ?? 0;
            net += sale.RoundingAmount ?? 0;

            return Math.Max(net, 0);
        }
    }
}
