using System.Threading.Tasks;
using JM.UI.DataService.DAL.Todo;
using JM.UI.Entities.Model.Todo;

namespace JM.UI.Service.Todo
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repository;

        public TodoService(ITodoRepository repository)
        {
            _repository = repository;
        }

        public Task<TodoCurrentUserDTO?> GetMe() => _repository.GetMe();

        public Task<TodoLookupsDTO> GetLookups() => _repository.GetLookups();

        public Task<TodoDashboardDTO> GetDashboard(
            int? status, int? flag, string? search, bool mineOnly,
            bool overdueOnly, bool dueTodayOnly, int page, int pageSize, int? storeId = null)
            => _repository.GetDashboard(status, flag, search, mineOnly, overdueOnly, dueTodayOnly, page, pageSize, storeId);

        public Task<TodoTaskDetailDTO?> GetTask(int taskId) => _repository.GetTask(taskId);

        public Task<TodoReadStateDTO> MarkRead(int taskId) => _repository.MarkRead(taskId);

        public Task<TodoResultDTO> CreateTask(TodoTaskSaveDTO dto) => _repository.CreateTask(dto);

        public Task<TodoResultDTO> UpdateTask(TodoTaskSaveDTO dto) => _repository.UpdateTask(dto);

        public Task<TodoResultDTO> PostUpdate(int taskId, string remarks) => _repository.PostUpdate(taskId, remarks);

        public Task<TodoResultDTO> MarkDone(int taskId, string? remarks) => _repository.MarkDone(taskId, remarks);

        public Task<TodoResultDTO> Reopen(int taskId) => _repository.Reopen(taskId);

        public Task<TodoResultDTO> DeleteTask(int taskId) => _repository.DeleteTask(taskId);

        public Task<TodoResultDTO> RestoreTask(int deletedTaskId) => _repository.RestoreTask(deletedTaskId);

        public Task<TodoPagedDTO<TodoDeletedTaskDTO>> GetDeleted(int page, int pageSize)
            => _repository.GetDeleted(page, pageSize);

        public Task<TodoPagedDTO<TodoActivityLogDTO>> GetActivity(int? userId, int page, int pageSize, string? action = null)
            => _repository.GetActivity(userId, page, pageSize, action);

        public Task LogSignIn() => _repository.LogSignIn();
    }
}
