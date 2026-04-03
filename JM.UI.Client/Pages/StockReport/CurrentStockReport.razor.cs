using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.StockReport_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.StockReport
{
    public partial class CurrentStockReportComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected CurrentStockReportResponseDTO ReportData { get; set; } = new();
        protected CurrentStockReportFilterDTO Filter { get; set; } = new();

        protected IEnumerable<StoreDTO> Stores { get; set; } = new List<StoreDTO>();
        protected IEnumerable<GroupModelDTO> Groups { get; set; } = new List<GroupModelDTO>();
        protected IEnumerable<string> ProductTypes { get; set; } = new List<string>
        {
            "Saleable", "Consumable", "Raw Material"
        };

        protected bool IsLoading { get; set; } = false;
        protected HashSet<string> ExpandedGroups { get; set; } = new();

        // ── Lifecycle ────────────────────────────────────────────────
        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadLookups();
            await LoadReport();
        }

        // ── Lookups ──────────────────────────────────────────────────
        private async Task LoadLookups()
        {
            try
            {
                Stores = await _serviceUnitOfWork.StoreService.GetStores()
                         ?? new List<StoreDTO>();

                Groups = await _serviceUnitOfWork.GroupService.GetGroups()
                         ?? new List<GroupModelDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load filter data: {ex.Message}");
            }
        }

        // ── Report Load ──────────────────────────────────────────────
        protected async Task LoadReport()
        {
            try
            {
                IsLoading = true;
                ReportData = await _serviceUnitOfWork.CurrentStockReportService
                                   .GetCurrentStockReport(Filter);

                // Expand all groups by default
                ExpandedGroups = ReportData.Groups
                    .Select(g => GroupKey(g))
                    .ToHashSet();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load stock report: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        // ── Filter Actions ───────────────────────────────────────────
        protected async Task ApplyFilter() => await LoadReport();

        protected async Task ResetFilter()
        {
            Filter = new CurrentStockReportFilterDTO();
            await LoadReport();
        }

        // ── Group Expand / Collapse ──────────────────────────────────
        protected void ToggleGroup(CurrentStockReportGroupDTO group)
        {
            var key = GroupKey(group);
            if (ExpandedGroups.Contains(key)) ExpandedGroups.Remove(key);
            else ExpandedGroups.Add(key);
        }

        protected bool IsExpanded(CurrentStockReportGroupDTO group)
            => ExpandedGroups.Contains(GroupKey(group));

        protected string GroupKey(CurrentStockReportGroupDTO group)
            => $"{group.GroupName}|{group.ProductType}";

        // ── Format Helpers ───────────────────────────────────────────
        protected string FormatQty(decimal qty, string? uom = null)
            => _serviceUnitOfWork.CurrentStockReportService.FormatQty(qty, uom);

        protected string FormatCurrency(decimal amount)
            => _serviceUnitOfWork.CurrentStockReportService.FormatCurrency(amount);
    }
}
