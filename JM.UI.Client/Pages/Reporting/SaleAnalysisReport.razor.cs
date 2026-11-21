using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.Groups;
using JM.UI.Entities.Model.SubGroups;
using JM.UI.Entities.Model.ItemOrigin;
using JM.UI.Entities.Model.Colors;
using JM.UI.Entities.Model.Designs;
using JM.UI.Entities.Model.ItemCatalogue;
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

public partial class SaleAnalysisReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public SaleAnalysisReportPdfService PdfService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState> AuthStateTask { get; set; } = default!;

    // â”€â”€ Filters â”€â”€
    protected DateTime? FromDate { get; set; } = DateTime.Today.AddDays(-30);
    protected DateTime? ToDate { get; set; } = DateTime.Today;

    protected List<StoreDTO> Stores { get; set; } = new();
    protected List<int> SelectedStoreIds { get; set; } = new();
    protected bool CanSelectAnyStore { get; set; }
    protected int CurrentUserId { get; set; }
    protected int UserStoreId { get; set; }

    protected List<GroupModelDTO> Groups { get; set; } = new();
    protected int? SelectedGroupId { get; set; }
    protected List<SubGroupModelDTO> SubGroups { get; set; } = new();
    protected int? SelectedSubGroupId { get; set; }
    protected List<ItemOriginDTO> Origins { get; set; } = new();
    protected int? SelectedOriginId { get; set; }
    protected List<ColorsDTO> Colors { get; set; } = new();
    protected int? SelectedColorId { get; set; }
    protected List<ItemCatalogueDTO> Catalogues { get; set; } = new();
    protected int? SelectedCatalogueId { get; set; }
    protected List<DesignModelDTO> Designs { get; set; } = new();
    protected int? SelectedDesignId { get; set; }

    protected string ProductTerm { get; set; } = string.Empty;
    protected string FeaturesTerm { get; set; } = string.Empty;
    protected decimal MinPrice { get; set; }
    protected decimal MaxPrice { get; set; }

    protected int FastThreshold { get; set; } = 30;
    protected int SlowThreshold { get; set; } = 5;

    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    // â”€â”€ Data â”€â”€
    protected SaleAnalysisReportDTO Report { get; set; } = new();
    protected bool HasGenerated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        CurrentUserId = await GetLocalStorageInt("UserId");
        UserStoreId = await GetLocalStorageInt("StoreId");
        try { var r = await _localStorage.GetAsync<string>("UserName"); if (r.Success && !string.IsNullOrWhiteSpace(r.Value)) CurrentUserName = r.Value; } catch { }

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

        await LoadStores();
        await LoadLookups();
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

    protected async Task LoadStores()
    {
        try
        {
            var stores = await _serviceUnitOfWork.StoreService.GetStores() ?? new List<StoreDTO>();
            if (CanSelectAnyStore) Stores = stores.ToList();
            else Stores = stores.Where(s => s.Id == UserStoreId).ToList();
            SelectedStoreIds = Stores.Select(s => s.Id).ToList();
        }
        catch { Stores = new List<StoreDTO>(); SelectedStoreIds = new List<int>(); }
    }

    protected async Task LoadLookups()
    {
        try
        {
            Groups = (await _serviceUnitOfWork.GroupService.GetGroups()).ToList();
        }
        catch { Groups = new List<GroupModelDTO>(); }
        try
        {
            SubGroups = (await _serviceUnitOfWork.SubGroupService.GetSubGroups()).ToList();
        }
        catch { SubGroups = new List<SubGroupModelDTO>(); }
        try
        {
            Origins = (await _serviceUnitOfWork.ItemOriginService.GetItemOrigins()).ToList();
        }
        catch { Origins = new List<ItemOriginDTO>(); }
        try
        {
            Colors = (await _serviceUnitOfWork.ColorsService.GetColorss()).ToList();
        }
        catch { Colors = new List<ColorsDTO>(); }
        try
        {
            Catalogues = (await _serviceUnitOfWork.ItemCatalogueService.GetItemCatalogues()).ToList();
        }
        catch { Catalogues = new List<ItemCatalogueDTO>(); }
        try
        {
            Designs = (await _serviceUnitOfWork.DesignService.GetDesigns()).ToList();
        }
        catch { Designs = new List<DesignModelDTO>(); }
    }

    protected void ToggleStore(int storeId, bool isChecked)
    {
        if (isChecked) { if (!SelectedStoreIds.Contains(storeId)) SelectedStoreIds.Add(storeId); }
        else SelectedStoreIds.Remove(storeId);
    }
    protected void ToggleAllStores(bool isChecked)
    {
        if (isChecked) SelectedStoreIds = Stores.Select(s => s.Id).ToList();
        else SelectedStoreIds.Clear();
    }
    protected bool AllStoresSelected =>
        Stores.Count > 0 && Stores.All(s => SelectedStoreIds.Contains(s.Id));

    protected async Task Generate()
    {
        if (!FromDate.HasValue || !ToDate.HasValue)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Please select From and To dates.");
            return;
        }
        if (SelectedStoreIds.Count == 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Branch", "Please select at least one branch.");
            return;
        }
        if (FastThreshold <= 0)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", "Fast threshold must be greater than 0.");
            return;
        }

        IsLoading = true;
        try
        {
            var storeIds = string.Join(",", SelectedStoreIds);
            Report = await _serviceUnitOfWork.ReportingService.GetSaleAnalysis(
                FromDate, ToDate, storeIds,
                SelectedGroupId, SelectedSubGroupId, SelectedDesignId,
                SelectedOriginId, SelectedColorId, SelectedCatalogueId,
                string.IsNullOrWhiteSpace(FeaturesTerm) ? null : FeaturesTerm,
                string.IsNullOrWhiteSpace(ProductTerm) ? null : ProductTerm,
                MinPrice == 0 ? (decimal?)null : MinPrice, MaxPrice == 0 ? (decimal?)null : MaxPrice, FastThreshold, SlowThreshold);
            HasGenerated = true;
        }
        catch (Exception ex)
        {
            Report = new SaleAnalysisReportDTO();
            HasGenerated = false;
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task PrintReport()
    {
        if (!HasGenerated) { notificationService.Notify(NotificationSeverity.Warning, "No Data", "Generate the report first."); return; }
        IsPrinting = true;
        try
        {
            var pdfBytes = PdfService.Generate(Report ?? new SaleAnalysisReportDTO(), CurrentUserName ?? "User");
            await JS.InvokeVoidAsync("downloadFileFromBytes", "SaleAnalysisReport.pdf", "application/pdf", pdfBytes);
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", "PDF generation failed: " + ex.Message);
        }
        finally { IsPrinting = false; StateHasChanged(); }
    }

    protected string CurrentUserName { get; set; } = "User";

    protected string ClassBadge(string c) => c switch
    {
        "Fast" => "background:#dcfce7;color:#166534;",
        "Slow" => "background:#fee2e2;color:#991b1b;",
        _ => "background:#e2e8f0;color:#334155;"
    };
}

