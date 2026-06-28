using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.DataService.DAL.Items
{
    public interface ITransferTypeRepository
    {
        Task<IEnumerable<TransferTypeDTO>> GetTransferTypes();
    }
}
