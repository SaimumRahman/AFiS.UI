using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Reporting_D;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Reporting
{
    public class ReportingService : IReportingService
    {
        private readonly IRepositoryUnitOfWork _repositoryUnitOfWork;

        public ReportingService(IRepositoryUnitOfWork repositoryUnitOfWork)
        {
            _repositoryUnitOfWork = repositoryUnitOfWork;
        }

        public async Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId, DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetProfitLossReport(storeId, fromDate, toDate);

        public async Task<IEnumerable<BookingReportDTO>> GetBookingReport(int? storeId, DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetBookingReport(storeId, fromDate, toDate);

        public async Task<IEnumerable<CustomerWiseSalesDetailDTO>> GetCustomerWiseSalesDetail(int? storeId, int? customerId, DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetCustomerWiseSalesDetail(storeId, customerId, fromDate, toDate);

        public async Task<IEnumerable<ExchangeReportDTO>> GetExchangeReport(int? storeId, DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetExchangeReport(storeId, fromDate, toDate);

        public async Task<IEnumerable<GroupWiseSalesSummaryDTO>> GetGroupWiseSalesSummary(DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetGroupWiseSalesSummary(fromDate, toDate);

        public async Task<IEnumerable<GroupWiseSalesDetailsDTO>> GetGroupWiseSalesDetails(DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetGroupWiseSalesDetails(fromDate, toDate);

        public async Task<IEnumerable<InvoiceWiseSaleSummaryDTO>> GetInvoiceWiseSaleSummary(DateTime? fromDate, DateTime? toDate)
            => await _repositoryUnitOfWork.ReportingRepository.GetInvoiceWiseSaleSummary(fromDate, toDate);

        public async Task<IEnumerable<TrialBalanceDTO>> GetTrialBalance(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode)
            => await _repositoryUnitOfWork.ReportingRepository.GetTrialBalance(companyCode, fromDate, toDate, branchCode);

        public async Task<IEnumerable<GeneralLedgerDTO>> GetGeneralLedger(string? companyCode, DateTime? fromDate, DateTime? toDate, string? branchCode, int? accountId, int? voucherTypeId)
            => await _repositoryUnitOfWork.ReportingRepository.GetGeneralLedger(companyCode, fromDate, toDate, branchCode, accountId, voucherTypeId);

        public async Task<IEnumerable<PartyBalanceDTO>> GetPartyBalance(string? companyCode, string? partyType)
            => await _repositoryUnitOfWork.ReportingRepository.GetPartyBalance(companyCode, partyType);

        public async Task<IEnumerable<BalanceRollupDTO>> GetBalanceRollup(string? companyCode, int? periodId)
            => await _repositoryUnitOfWork.ReportingRepository.GetBalanceRollup(companyCode, periodId);
    }
}