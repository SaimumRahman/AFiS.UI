using JM.UI.Client.Pages.Dialog.Sms;
using JM.UI.Client.Pages.Dialog.Audit;
using JM.UI.Entities.Model.Audit;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JM.UI.Client.Pages.Audit
{
    public partial class AuditLog : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService dialogService { get; set; } = default!;

        protected AuditLogPageResult Result = new();
        protected List<AuditLogDTO> Items = new();
        protected DateTime? FromDate;
        protected DateTime? ToDate;
        protected string SearchText = string.Empty;
        protected string? TableName;
        protected bool IsLoading;
        protected int Page = 1;
        protected int PageSize = 25;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            ToDate = DateTime.Today;
            FromDate = DateTime.Today.AddDays(-7);
            await LoadLog();
        }

        protected async Task LoadLog()
        {
            IsLoading = true;
            try
            {
                var r = await _serviceUnitOfWork.AuditLogService.GetAsync(TableName, null, null, null, FromDate, ToDate, SearchText, Page, PageSize);
                Result = r ?? new AuditLogPageResult();
                Items = Result.Items ?? new List<AuditLogDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load audit log: {ex.Message}");
            }
            finally { IsLoading = false; }
        }

        protected async Task OnSearch() { Page = 1; await LoadLog(); }

        protected string Pretty(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "-";
            try { using var doc = JsonDocument.Parse(json); return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }); }
            catch { return json; }
        }

        protected async Task ViewDetail(AuditLogDTO row)
        {
            await dialogService.OpenAsync<AuditLogDetailDialog>("Audit Detail", new Dictionary<string, object> { { "Row", row } },
                new DialogOptions { Width = "900px", Height = "600px" });
        }
    }
}
