using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Suppliers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Suppliers
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierModelDTO>> GetSuppliers();
        Task<SupplierModelDTO?> GetSupplierById(int id);
        Task<ResponseResult> SaveUpdateSupplier(SupplierModelDTO supplier);
        Task<ResponseResult> DeleteSupplier(int id);
    }
}
