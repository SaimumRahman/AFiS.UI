using JM.UI.DataService.DAL.Audit;
using JM.UI.DataService.DAL.UnitOfWork;
using JM.UI.Entities.Model.Audit;
using System;
using System.Threading.Tasks;

namespace JM.UI.Service.Audit
{
    public interface IAuditLogService
    {
        Task<AuditLogPageResult> GetAsync(string? tableName, long? rowId, string? businessKey, int? storeId, DateTime? fromDate, DateTime? toDate, string? searchTerm, int page, int pageSize);
        Task<AuditLogDTO> GetByIdAsync(long id);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IRepositoryUnitOfWork _uow;
        public AuditLogService(IRepositoryUnitOfWork uow) { _uow = uow; }
        public Task<AuditLogPageResult> GetAsync(string? tableName, long? rowId, string? businessKey, int? storeId, DateTime? fromDate, DateTime? toDate, string? searchTerm, int page, int pageSize)
            => _uow.AuditLogRepository.GetAsync(tableName, rowId, businessKey, storeId, fromDate, toDate, searchTerm, page, pageSize);
        public Task<AuditLogDTO> GetByIdAsync(long id) => _uow.AuditLogRepository.GetByIdAsync(id);
    }
}
