using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SupplierPayments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.SupplierPayments
{
    public interface ISupplierPaymentRepository
    {
        Task<IEnumerable<SupplierPaymentDTO>> GetSupplierPayments();
        Task<SupplierPaymentDTO?> GetSupplierPaymentById(int id);
        Task<ResponseResult> SaveUpdateSupplierPayment(SupplierPaymentDTO payment);
        Task DeleteSupplierPayment(int id);
        Task<IEnumerable<SupplierLedgerDTO>> GetSupplierLedger(int supplierId);
        Task<IEnumerable<SupplierOutstandingDTO>> GetSupplierOutstanding();
    }
}
