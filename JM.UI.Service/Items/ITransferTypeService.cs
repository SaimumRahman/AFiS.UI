using JM.UI.Entities.Model.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Items
{
    public interface ITransferTypeService
    {
        Task<IEnumerable<TransferTypeDTO>> GetTransferTypes();
    }
}
