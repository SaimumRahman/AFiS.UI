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

        public async Task<IEnumerable<ProfitLossReportDTO>> GetProfitLossReport(int? storeId)
            => await _repositoryUnitOfWork.ReportingRepository.GetProfitLossReport(storeId);
    }
}
