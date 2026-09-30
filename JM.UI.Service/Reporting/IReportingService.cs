using JM.UI.Entities.Model.Reporting_D;
using System;

namespace JM.UI.Service.Reporting
{
    public interface IReportingService
    {
        Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<BookingReportDTO>> GetBookingReport(int? storeId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<CustomerWiseSalesDetailDTO>> GetCustomerWiseSalesDetail(int? storeId, int? customerId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<ExchangeReportDTO>> GetExchangeReport(int? storeId, DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<GroupWiseSalesSummaryDTO>> GetGroupWiseSalesSummary(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<GroupWiseSalesDetailsDTO>> GetGroupWiseSalesDetails(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<InvoiceWiseSaleSummaryDTO>> GetInvoiceWiseSaleSummary(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<PayTypeWiseSalesDetailsDTO>> GetPayTypeWiseSalesDetails(DateTime? fromDate, DateTime? toDate);
        Task<IEnumerable<TrialBalanceDTO>> GetTrialBalance(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode);
        Task<IEnumerable<GeneralLedgerDTO>> GetGeneralLedger(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode, int? accountId, int? voucherTypeId);
        Task<IEnumerable<PartyBalanceDTO>> GetPartyBalance(string? companyCode, string? partyType);
        Task<IEnumerable<BalanceRollupDTO>> GetBalanceRollup(string? companyCode, int? periodId);
    }
}