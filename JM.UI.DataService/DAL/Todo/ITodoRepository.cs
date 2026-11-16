using System.Collections.Generic;
using System.Threading.Tasks;
using JM.UI.Entities.Model.Todo;

namespace JM.UI.DataService.DAL.Todo
{
    public interface ITodoRepository
    {
        Task<TodoCurrentUserDTO?> GetMe();
        Task<TodoLookupsDTO> GetLookups();

        Task<TodoDashboardDTO> GetDashboard(
            int? status, int? flag, string? search, bool mineOnly,
            bool overdueOnly, bool dueTodayOnly, int page, int pageSize);

        Task<TodoTaskDetailDTO?> GetTask(int taskId);

        Task<TodoResultDTO> CreateTask(TodoTaskSaveDTO dto);
        Task<TodoResultDTO> UpdateTask(TodoTaskSaveDTO dto);
        Task<TodoResultDTO> PostUpdate(int taskId, string remarks);
        Task<TodoResultDTO> MarkDone(int taskId, string? remarks);
        Task<TodoResultDTO> Reopen(int taskId);
        Task<TodoResultDTO> DeleteTask(int taskId);
        Task<TodoResultDTO> RestoreTask(int deletedTaskId);

        Task<TodoPagedDTO<TodoDeletedTaskDTO>> GetDeleted(int page, int pageSize);
        Task<TodoPagedDTO<TodoActivityLogDTO>> GetActivity(int? userId, int page, int pageSize);

        Task LogSignIn();
    }
}
