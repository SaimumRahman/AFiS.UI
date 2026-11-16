using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JM.UI.Entities.Model.Todo;
using JM.UI.Entities.Services;
using Microsoft.Extensions.Logging;

namespace JM.UI.DataService.DAL.Todo
{
    public class TodoRepository : BaseRepository, ITodoRepository
    {
        public TodoRepository(
            IHttpClientFactory httpClientFactory,
            ITokenProvider tokenProvider,
            ILogger<TodoRepository> logger)
            : base(httpClientFactory, tokenProvider, logger)
        {
        }

        // ---------------------------------------------------------------- reads

        public async Task<TodoCurrentUserDTO?> GetMe()
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync("api/Todo/me");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<TodoCurrentUserDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading current To-Do user");
                return null;
            }
        }

        public async Task<TodoLookupsDTO> GetLookups()
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync("api/Todo/lookups");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TodoLookupsDTO>() ?? new TodoLookupsDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading To-Do lookups");
                throw;
            }
        }

        public async Task<TodoDashboardDTO> GetDashboard(
            int? status, int? flag, string? search, bool mineOnly,
            bool overdueOnly, bool dueTodayOnly, int page, int pageSize)
        {
            try
            {
                var url = "api/Todo/dashboard";
                var q = new List<string>();
                if (status.HasValue) q.Add($"status={status.Value}");
                if (flag.HasValue) q.Add($"flag={flag.Value}");
                if (!string.IsNullOrWhiteSpace(search)) q.Add($"search={Uri.EscapeDataString(search)}");
                q.Add($"mineOnly={mineOnly.ToString().ToLowerInvariant()}");
                q.Add($"overdueOnly={overdueOnly.ToString().ToLowerInvariant()}");
                q.Add($"dueTodayOnly={dueTodayOnly.ToString().ToLowerInvariant()}");
                q.Add($"page={page}");
                q.Add($"pageSize={pageSize}");
                url += "?" + string.Join("&", q);

                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TodoDashboardDTO>() ?? new TodoDashboardDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading To-Do dashboard");
                throw;
            }
        }

        public async Task<TodoTaskDetailDTO?> GetTask(int taskId)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync($"api/Todo/get/{taskId}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<TodoTaskDetailDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading To-Do task {TaskId}", taskId);
                return null;
            }
        }

        public async Task<TodoPagedDTO<TodoDeletedTaskDTO>> GetDeleted(int page, int pageSize)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync($"api/Todo/deleted?page={page}&pageSize={pageSize}");
                if (!response.IsSuccessStatusCode) return new TodoPagedDTO<TodoDeletedTaskDTO>();
                return await response.Content.ReadFromJsonAsync<TodoPagedDTO<TodoDeletedTaskDTO>>()
                       ?? new TodoPagedDTO<TodoDeletedTaskDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading deleted To-Do tasks");
                throw;
            }
        }

        public async Task<TodoPagedDTO<TodoActivityLogDTO>> GetActivity(int? userId, int page, int pageSize)
        {
            try
            {
                var url = $"api/Todo/activity?page={page}&pageSize={pageSize}";
                if (userId.HasValue) url += $"&userId={userId.Value}";

                var client = GetAuthenticatedClient("MainApi");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new TodoPagedDTO<TodoActivityLogDTO>();
                return await response.Content.ReadFromJsonAsync<TodoPagedDTO<TodoActivityLogDTO>>()
                       ?? new TodoPagedDTO<TodoActivityLogDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading To-Do activity log");
                throw;
            }
        }

        // ---------------------------------------------------------------- writes

        public async Task<TodoResultDTO> CreateTask(TodoTaskSaveDTO dto)
            => await PostResult("api/Todo/create", dto, "create task");

        public async Task<TodoResultDTO> UpdateTask(TodoTaskSaveDTO dto)
            => await PostResult("api/Todo/update", dto, "update task");

        public async Task<TodoResultDTO> PostUpdate(int taskId, string remarks)
            => await PostResult("api/Todo/updates", new TodoPostUpdateDTO { TaskId = taskId, Remarks = remarks }, "post update");

        public async Task<TodoResultDTO> MarkDone(int taskId, string? remarks)
            => await PostResult("api/Todo/done", new TodoPostUpdateDTO { TaskId = taskId, Remarks = remarks ?? string.Empty }, "complete task");

        public async Task<TodoResultDTO> Reopen(int taskId)
            => await PostResult<object>($"api/Todo/reopen/{taskId}", null, "reopen task");

        public async Task<TodoResultDTO> DeleteTask(int taskId)
            => await PostResult<object>($"api/Todo/delete/{taskId}", null, "delete task");

        public async Task<TodoResultDTO> RestoreTask(int deletedTaskId)
            => await PostResult<object>($"api/Todo/restore/{deletedTaskId}", null, "restore task");

        public async Task LogSignIn()
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                await client.PostAsync("api/Todo/sign-in", null);
            }
            catch (Exception ex)
            {
                // A failed audit write must never block the sign-in itself.
                _logger.LogWarning(ex, "Could not write the To-Do sign-in activity log");
            }
        }

        /// <summary>
        /// Posts a request and reads the server's <see cref="TodoResultDTO"/> even for
        /// 400/403/409 so the page can show the server's own message instead of a
        /// generic HTTP error.
        /// </summary>
        private async Task<TodoResultDTO> PostResult<TBody>(string url, TBody? body, string what)
        {
            try
            {
                var client = GetAuthenticatedClient("MainApi");
                HttpResponseMessage response;
                if (body is null)
                    response = await client.PostAsync(url, null);
                else
                    response = await client.PostAsJsonAsync(url, body);

                try
                {
                    var result = await response.Content.ReadFromJsonAsync<TodoResultDTO>();
                    if (result != null) return result;
                }
                catch
                {
                    // fall through to the generic message
                }

                return new TodoResultDTO
                {
                    Success = false,
                    Message = response.IsSuccessStatusCode
                        ? "Done."
                        : $"Could not {what} (HTTP {(int)response.StatusCode})."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to {What}", what);
                return new TodoResultDTO { Success = false, Message = ex.Message };
            }
        }
    }
}
