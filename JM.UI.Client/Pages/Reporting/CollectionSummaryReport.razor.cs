using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class CollectionSummaryReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public CollectionSummaryReportPdfService PdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }

    protected List<StoreDTO> Stores { get; set; } = new();
    protected int? SelectedStoreId { get; set; }

    protected bool CanSelectAnyStore { get; set; }
    protected int CurrentUserId { get; set; }
    protected int UserStoreId { get; set; }
    protected string CurrentUserName { get; set; } = "-";

    protected string SelectedSourceType { get; set; } = "ALL";
    protected string SelectedChannel { get; set; } = "ALL";
    protected string SearchText { get; set; } = string.Empty;
    protected bool ShowDetails { get; set; } = false;

    protected CollectionSummaryReportDTO Report { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }
    protected bool HasGenerated { get; set; }

    protected HashSet<int> ExpandedIds { get; set; } = new();

    protected string SelectedStoreName =>
        Stores.FirstOrDefault(s => s.Id == SelectedStoreId)?.Name ?? "-";

    protected IEnumerable<string> SourceTypes => new[] { "ALL", "POS Sale", "Booking Advance", "Booking Final", "Tailoring / Due" };

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        UserStoreId = await GetLocalStorageInt("StoreId");
        CurrentUserName = await GetLocalStorageString("UserName") ?? "-";

        CanSelectAnyStore = CurrentUserId == 1;
        if (!CanSelectAnyStore)
        {
            try
            {
                CanSelectAnyStore = await _serviceUnitOfWork.UserGroupService
                                        .GetSystemGroupCountByUserId(CurrentUserId) > 0;
            }
            catch { CanSelectAnyStore = false; }
        }

        var today = DateTime.Today;
        FromDate = today;
        ToDate = today;

        await LoadStores();

        if (!CanSelectAnyStore)
            SelectedStoreId = UserStoreId > 0 ? UserStoreId : Stores.FirstOrDefault()?.Id;
    }

    private async Task<int> GetLocalStorageInt(string key)
    {
        try
        {
            var r = await _localStorage.GetAsync<string>(key);
            if (r.Success && int.TryParse(r.Value, out var v)) return v;
        }
        catch { }
        return 0;
    }

    private async Task<string?> GetLocalStorageString(string key)
    {
        try
        {
            var r = await _localStorage.GetAsync<string>(key);
            if (r.Success) return r.Value;
        }
        catch { }
        return null;
    }

    protected async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
            Stores = CanSelectAnyStore
                ? stores.ToList()
                : stores.Where(s => s.Id == UserStoreId).ToList();
            SelectedStoreId = Stores.FirstOrDefault()?.Id;
        }
        catch { Stores = new List<StoreDTO>(); }
    }

    protected async Task Generate()
    {
        if (SelectedStoreId is null or 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Branch", "Please select a branch first.");
            return;
        }
        IsLoading = true;
        StateHasChanged();
        try
        {
            Report = await _serviceUnitOfWork.ReportingService.GetCollectionSummary(
                FromDate, ToDate, SelectedStoreId, SelectedSourceType, SelectedChannel, SearchText) ?? new CollectionSummaryReportDTO();
            HasGenerated = true;
            ExpandedIds.Clear();
        }
        catch (Exception ex)
        {
            Report = new CollectionSummaryReportDTO();
            notificationService.Notify(NotificationSeverity.Error, "Failed", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected void ClearFilters()
    {
        var today = DateTime.Today;
        FromDate = today;
        ToDate = today;
        SelectedSourceType = "ALL";
        SelectedChannel = "ALL";
        SearchText = string.Empty;
        ShowDetails = false;
    }

    protected void ToggleRow(int id)
    {
        if (ExpandedIds.Contains(id)) ExpandedIds.Remove(id);
        else ExpandedIds.Add(id);
    }

    protected decimal TotalNet => Report.Rows.Sum(r => r.NetPaid);
    protected int TotalCount => Report.Rows.Count;

    protected decimal ChannelTotal(string ch) => Report.Rows.SelectMany(r => r.Payments).Where(p => p.Channel == ch).Sum(p => p.Amount);
    protected decimal CashTotal => ChannelTotal("Cash");
    protected decimal BkashTotal => ChannelTotal("bKash");
    protected decimal NagadTotal => ChannelTotal("Nagad");
    protected decimal CityTotal => ChannelTotal("CityPOS");
    protected decimal DbblTotal => ChannelTotal("DBBLPOS");

    protected IEnumerable<CollectionPaymentDTO> AllPayments => Report.Rows.SelectMany(r => r.Payments);
    protected decimal TotalCash => AllPayments.Where(p => p.ChannelGroup == "Cash").Sum(p => p.Amount);
    protected decimal TotalMfs => AllPayments.Where(p => p.ChannelGroup == "MFS").Sum(p => p.Amount);
    protected decimal TotalBank => AllPayments.Where(p => p.ChannelGroup == "Bank").Sum(p => p.Amount);
    protected decimal TotalPhysical => TotalCash + TotalMfs + TotalBank;

    protected static string Money(decimal value) => value.ToString("N0");
    protected static string Money2(decimal value) => value.ToString("N2");

    protected async Task PrintReport()
    {
        if (!Report.Rows.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "Generate the report first.");
            return;
        }
        try
        {
            IsPrinting = true;
            StateHasChanged();
            var pdfBytes = PdfService.Generate(Report, SelectedStoreName, FromDate, ToDate, CurrentUserName);
            var fileName = $"CollectionSummary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);
            notificationService.Notify(NotificationSeverity.Success, "Success", "Report generated.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed", $"Could not generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }
}