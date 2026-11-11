using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.Employees;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.ItemCatalogue;
using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SubGroups;
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

public partial class SalesBookingReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public SalesBookingReportPdfService SalesBookingReportPdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    // ── Filters ──
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }

    protected List<StoreDTO> Stores { get; set; } = new();
    protected int? SelectedStoreId { get; set; }

    protected List<GroupModelDTO> Groups { get; set; } = new();
    protected int? SelectedGroupId { get; set; }

    protected List<SubGroupModelDTO> AllSubGroups { get; set; } = new();
    protected int? SelectedSubGroupId { get; set; }

    protected List<DesignModelDTO> AllDesigns { get; set; } = new();
    protected int? SelectedDesignId { get; set; }

    protected List<ItemCatalogueDTO> Catalogues { get; set; } = new();
    protected int? SelectedCatalogueId { get; set; }

    protected List<EmployeeModelDTO> Employees { get; set; } = new();
    protected string? SelectedSalesmanName { get; set; }

    protected List<SalesBookingUserDTO> Users { get; set; } = new();
    protected int? SelectedServedByUserId { get; set; }

    // ── Permission / identity ──
    protected bool CanSelectAnyStore { get; set; }
    protected int CurrentUserId { get; set; }
    protected int UserStoreId { get; set; }
    protected string CurrentUserName { get; set; } = "-";

    // ── Result ──
    protected SalesBookingReportDTO Report { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }
    protected bool HasGenerated { get; set; }

    protected List<SubGroupModelDTO> SubGroups =>
        SelectedGroupId.HasValue
            ? AllSubGroups.Where(s => s.GroupId == SelectedGroupId.Value).ToList()
            : AllSubGroups;

    protected List<DesignModelDTO> Designs =>
        SelectedSubGroupId.HasValue
            ? AllDesigns.Where(d => d.SubGroupId == SelectedSubGroupId.Value).ToList()
            : AllDesigns;

    protected string SelectedStoreName =>
        Stores.FirstOrDefault(s => s.Id == SelectedStoreId)?.Name ?? "-";

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"{FromDate:dd-MMM-yyyy}  TO  {ToDate:dd-MMM-yyyy}"
            : "ALL TIME";

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
        await Task.WhenAll(LoadGroups(), LoadSubGroups(), LoadDesigns(), LoadCatalogues());
        await LoadServedByUsers();
        await LoadEmployees();

        if (!CanSelectAnyStore)
            SelectedStoreId = UserStoreId > 0 ? UserStoreId : Stores.FirstOrDefault()?.Id;

        SelectedServedByUserId = Users.Any(u => u.UserId == CurrentUserId) ? CurrentUserId : null;
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
        catch
        {
            Stores = new List<StoreDTO>();
        }
    }

    protected async Task LoadGroups()
    {
        try { Groups = (await _serviceUnitOfWork.GroupService.GetGroups())?.ToList() ?? new(); }
        catch { Groups = new(); }
    }

    protected async Task LoadSubGroups()
    {
        try { AllSubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups())?.ToList() ?? new(); }
        catch { AllSubGroups = new(); }
    }

    protected async Task LoadDesigns()
    {
        try { AllDesigns = (await _serviceUnitOfWork.DesignService.GetDesigns())?.ToList() ?? new(); }
        catch { AllDesigns = new(); }
    }

    protected async Task LoadCatalogues()
    {
        try { Catalogues = (await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues())?.ToList() ?? new(); }
        catch { Catalogues = new(); }
    }

    protected async Task LoadEmployees()
    {
        try
        {
            var storeId = SelectedStoreId ?? (UserStoreId > 0 ? UserStoreId : 0);
            var list = storeId > 0
                ? await _serviceUnitOfWork.EmployeeService.GetEmployeesByStoreId(storeId)
                : await _serviceUnitOfWork.EmployeeService.GetEmployees();

            Employees = list?.ToList() ?? new();
            if (Employees.Count == 0 && storeId > 0)
                Employees = (await _serviceUnitOfWork.EmployeeService.GetEmployees())?.ToList() ?? new();
        }
        catch { Employees = new(); }
    }

    protected async Task LoadServedByUsers()
    {
        try
        {
            var list = await _serviceUnitOfWork.ReportingService.GetSalesBookingUsers(SelectedStoreId);
            Users = list?.ToList() ?? new();
        }
        catch { Users = new(); }
    }

    protected async Task OnStoreChanged()
    {
        SelectedSalesmanName = null;
        await Task.WhenAll(LoadEmployees(), LoadServedByUsers());
        StateHasChanged();
    }

    protected void OnGroupChanged()
    {
        SelectedSubGroupId = null;
        SelectedDesignId = null;
    }

    protected void OnSubGroupChanged()
    {
        SelectedDesignId = null;
    }

    protected async Task Generate()
    {
        if (SelectedStoreId is null or 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Branch",
                "Please select a branch first.");
            return;
        }

        IsLoading = true;
        StateHasChanged();
        try
        {
            Report = await _serviceUnitOfWork.ReportingService.GetSalesBookingReport(
                FromDate,
                ToDate,
                SelectedStoreId,
                SelectedGroupId,
                SelectedSubGroupId,
                SelectedDesignId,
                SelectedCatalogueId,
                SelectedSalesmanName,
                SelectedServedByUserId)
                ?? new SalesBookingReportDTO();

            HasGenerated = true;
        }
        catch (Exception ex)
        {
            Report = new SalesBookingReportDTO();
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
        SelectedGroupId = null;
        SelectedSubGroupId = null;
        SelectedDesignId = null;
        SelectedCatalogueId = null;
        SelectedSalesmanName = null;
        SelectedServedByUserId = Users.Any(u => u.UserId == CurrentUserId) ? CurrentUserId : null;
    }

    // ── Totals ──
    protected decimal TotalBill => Report.Invoices.Sum(i => i.TotalBill);
    protected decimal TotalCampaignDiscount => Report.Invoices.Sum(i => i.CampaignDiscount);
    protected decimal TotalInvoiceDiscount => Report.Invoices.Sum(i => i.InvoiceDiscount);
    protected decimal TotalNet => Report.Invoices.Sum(i => i.NetAmount);
    protected decimal TotalExchange => Report.Invoices.Sum(i => i.ExchangeAmount);
    protected decimal TotalPaid => Report.Invoices.Sum(i => i.TotalPaid);
    protected decimal TotalDue => Report.Invoices.Sum(i => i.TotalDue);

    protected decimal BookingTotalValue => Report.Bookings.Sum(b => b.TotalBill);
    protected decimal BookingTotalAdvance => Report.Bookings.Sum(b => b.AdvancePaid);
    protected decimal BookingTotalDue => Report.Bookings.Sum(b => b.DueBalance);

    protected IEnumerable<SalesBookingPaymentDTO> AllPayments =>
        Report.Invoices.SelectMany(i => i.Payments)
            .Concat(Report.Bookings.SelectMany(b => b.Payments));

    protected decimal CollectionCash => AllPayments.Where(p => p.PaymentType == "Cash").Sum(p => p.Amount);
    protected decimal CollectionMfs => AllPayments.Where(p => p.PaymentType == "MFS").Sum(p => p.Amount);
    protected decimal CollectionBank => AllPayments.Where(p => p.PaymentType == "Bank").Sum(p => p.Amount);
    protected decimal CollectionTotal => CollectionCash + CollectionMfs + CollectionBank;

    protected static string Money(decimal value) => value.ToString("N0");
    protected static string Money2(decimal value) => value.ToString("N2");

    protected static string CustomerLabel(int? customerId, string? name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return customerId is > 0 ? $"C-{customerId:D4}" : "Walk-in";
        return customerId is > 0 ? $"{trimmed} (C-{customerId:D4})" : trimmed;
    }

    protected async Task PrintReport()
    {
        if (!Report.Invoices.Any() && !Report.Bookings.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data",
                "Generate the report first.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            var pdfBytes = SalesBookingReportPdfService.GenerateSalesBookingReport(
                Report,
                SelectedStoreName,
                FromDate,
                ToDate,
                CurrentUserName);

            var fileName = $"SalesBooking_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success",
                "Sales & booking report generated.");
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Failed",
                $"Could not generate report: {ex.Message}");
        }
        finally
        {
            IsPrinting = false;
            StateHasChanged();
        }
    }
}
