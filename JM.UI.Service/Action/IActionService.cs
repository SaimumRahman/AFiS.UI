using JM.Infrastructure.Models;
using JM.UI.Entities.Model.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Service.Action
{
    public interface IActionService
    {
        Task<ResponseResult> CreateAction(ActionDTO action);
        Task<ResponseResult> DeleteAction(int actionId);
        Task<ActionDTO?> GetActionById(int actionId);
        Task<IEnumerable<ActionDTO>> GetAllActions();
        Task<ResponseResult> UpdateAction(ActionDTO action);
    }
}