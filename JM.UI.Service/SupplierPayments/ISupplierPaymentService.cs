using JM.Infrastructure.Models;
using JM.UI.Entities.Model.SupplierPayments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.SupplierPayments
{
    public interface ISupplierPaymentService
    {
        Task<IEnumerable<SupplierPaymentDTO>> GetSupplierPayments();
        Task<SupplierPaymentDTO?> GetSupplierPaymentById(int id);
        Task<ResponseResult> SaveUpdateSupplierPayment(SupplierPaymentDTO payment);
        Task<ResponseResult> DeleteSupplierPayment(int id);
        Task<IEnumerable<SupplierLedgerDTO>> GetSupplierLedger(int supplierId);
        Task<IEnumerable<SupplierOutstandingDTO>> GetSupplierOutstanding();
    }
}
