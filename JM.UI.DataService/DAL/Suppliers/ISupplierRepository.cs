using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Suppliers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Suppliers
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<SupplierModelDTO>> GetSuppliers();
        Task<SupplierModelDTO?> GetSupplierById(int id);
        Task<ResponseResult> SaveUpdateSupplier(SupplierModelDTO supplier);
        Task DeleteSupplier(int id);
    }
}
