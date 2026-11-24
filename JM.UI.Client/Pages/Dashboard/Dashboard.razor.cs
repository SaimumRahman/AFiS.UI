using JM.UI.Entities.Model.Dashboard;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Todo;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Dashboard
{
    public partial class DashboardComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;

        protected AdminDashboardDTO Data { get; set; } = new();
        protected List<StoreDTO> Stores { get; set; } = new();
        protected int? SelectedStoreId { get; set; }
        protected string Period { get; set; } = "Today";
        protected bool Loading { get; set; }

        protected List<TodoTaskListItemDTO> TodoItems { get; set; } = new();
        protected int TodoTotal { get; set; }

        protected string CurrentDateLabel => DateTime.Today.ToString("dddd, d MMMM yyyy", CultureInfo.GetCultureInfo("en-GB"));

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Stores = (await ServiceUnitOfWork.StoreService.GetStores())?
                    .Where(s => !string.Equals(s.Name?.Trim(), "Damaged", StringComparison.OrdinalIgnoreCase))
                    .ToList() ?? new List<StoreDTO>();
            }
            catch { }

            await Task.WhenAll(LoadDashboardAsync(), LoadTodoAsync());
        }

        protected async Task OnBranchChanged(object value)
        {
            if (value == null) { SelectedStoreId = null; }
            else if (int.TryParse(value.ToString(), out var id)) { SelectedStoreId = id <= 0 ? null : id; }
            await LoadDashboardAsync();
        }

        protected async Task OnPeriodChanged(string period)
        {
            if (Period == period) return;
            Period = period;
            await LoadDashboardAsync();
        }

        protected async Task LoadDashboardAsync()
        {
            Loading = true;
            try
            {
                Data = await ServiceUnitOfWork.DashboardService.GetAdminDashboardAsync(SelectedStoreId, Period)
                       ?? new AdminDashboardDTO();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally
            {
                Loading = false;
                StateHasChanged();
            }
        }

        protected async Task LoadTodoAsync()
        {
            try
            {
                var d = await ServiceUnitOfWork.TodoService.GetDashboard(0, null, null, false, false, false, 1, 5, null);
                TodoItems = d?.Items ?? new List<TodoTaskListItemDTO>();
                TodoTotal = d?.Counts?.Open ?? d?.TotalCount ?? 0;
            }
            catch { }
            finally { StateHasChanged(); }
        }

        // ── formatting helpers ────────────────────────────────────────────────
        private static readonly CultureInfo In = CultureInfo.GetCultureInfo("en-IN");

        protected string Fmt(decimal v) => "৳" + v.ToString("#,##,##0", In);
        protected string Num(decimal v) => v.ToString("#,##,##0", In);

        protected string Pct(decimal part, decimal total)
        {
            if (total <= 0) return "0%";
            return Math.Round(part / total * 100m, 1) + "%";
        }

        protected decimal Gross => Data.NetSales + Data.Discount;
        protected string AvgTicket => Data.Invoices > 0 ? Fmt(Data.NetSales / Data.Invoices) : Fmt(0);
        protected int BranchCount => Math.Max(Data.Branches.Count, 1);
        protected decimal TotalBranchNet => Data.Branches.Sum(b => b.Net);

        protected string ToneClass(string tone) => tone switch
        {
            "up" => "t-up",
            "down" => "t-down",
            _ => "t-mute"
        };

        protected string StatusClass(string status) => status switch
        {
            "Approved" => "p-ok",
            "Reopened" => "p-warn",
            _ => "p-warn"
        };

        protected string FlagClass(string flagName) => flagName switch
        {
            "Urgent" => "p-bad",
            "Important" => "p-warn",
            _ => "p-info"
        };

        protected string TodoScope(TodoTaskListItemDTO t)
        {
            if (t.IsPrivate) return "Personal";
            if (t.AllBranches) return "All branches";
            return t.CreatedByBranch ?? "Branch";
        }

        protected string TodoDue(TodoTaskListItemDTO t)
        {
            if (!t.DueAtUtc.HasValue) return string.Empty;
            var local = t.DueAtUtc.Value.AddHours(6);
            if (t.IsOverdue) return "Overdue";
            if (t.IsDueToday) return "Today, " + local.ToString("h:mm tt");
            return local.ToString("ddd, d MMM");
        }

        // ── trend chart helpers ───────────────────────────────────────────────
        protected List<(double X, double Y)> TrendPlot => ComputeTrendPlot();

        private List<(double X, double Y)> ComputeTrendPlot()
        {
            var res = new List<(double X, double Y)>();
            if (Data.Trend.Count == 0) return res;
            double max = (double)Math.Max(Data.Trend.Max(t => t.Amount), 1m);
            double x0 = 50, w = 570, yBottom = 200, yTop = 35;
            double h = yBottom - yTop;
            for (int i = 0; i < Data.Trend.Count; i++)
            {
                double x = Data.Trend.Count == 1 ? x0 : x0 + i * (w / (Data.Trend.Count - 1));
                double y = yBottom - ((double)Data.Trend[i].Amount / max) * h;
                res.Add((x, y));
            }
            return res;
        }

        protected string TrendPoints => string.Join(" ", TrendPlot.Select(p => $"{p.X:0},{p.Y:0}"));

        protected Microsoft.AspNetCore.Components.MarkupString TrendXLabels()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Data.Trend.Count; i++)
            {
                var x = Data.Trend.Count == 1 ? 50 : (int)(50 + i * (570.0 / (Data.Trend.Count - 1)));
                sb.Append($"<text x=\"{x}\" y=\"226\" text-anchor=\"middle\" font-size=\"12\" fill=\"#5B636D\" font-family=\"Inter, system-ui, sans-serif\">{System.Net.WebUtility.HtmlEncode(Data.Trend[i].Label)}</text>");
            }
            return (Microsoft.AspNetCore.Components.MarkupString)sb.ToString();
        }

        protected double CashPct => Data.PaymentMix.Total <= 0 ? 0 : (double)(Data.PaymentMix.Cash / Data.PaymentMix.Total) * 100;
        protected double MfsPct => Data.PaymentMix.Total <= 0 ? 0 : (double)(Data.PaymentMix.Mfs / Data.PaymentMix.Total) * 100;
        protected double BankPct => Data.PaymentMix.Total <= 0 ? 0 : (double)(Data.PaymentMix.Bank / Data.PaymentMix.Total) * 100;
    }
}
