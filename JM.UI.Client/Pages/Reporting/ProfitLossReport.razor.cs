using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class ProfitLossReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<ProfitLossReportDTO> ReportGrid = default!;
    protected IEnumerable<ProfitLossReportDTO> Report { get; set; } = new List<ProfitLossReportDTO>();
    protected bool IsLoading { get; set; }

    protected List<StoreDTO> Stores { get; set; } = new();
    protected int? SelectedStoreId { get; set; }

    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }

    protected decimal TotalInSum => Report.Sum(x => x.TotalIn);
    protected decimal TotalOutSum => Report.Sum(x => x.TotalOut);
    protected decimal TotalSaleSum => Report.Sum(x => x.TotalSaleAmount);
    protected decimal TotalProfitSum => Report.Sum(x => x.Profit);
    protected decimal CurrentStockSum => Report.Sum(x => x.CurrentStock);
    protected decimal TotalPurchaseValue => Report.Sum(x => x.PurchaseValue);

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadStores();
        await LoadReport();
    }

    private async Task LoadStores()
    {
        try
        {
            Stores = (await _serviceUnitOfWork.StoreService.GetStores())?.ToList() ?? new List<StoreDTO>();
        }
        catch
        {
            Stores = new List<StoreDTO>();
        }
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            var result = await _serviceUnitOfWork.ReportingService.GetProfitLossReport(SelectedStoreId, FromDate, ToDate);
            Report = result ?? new List<ProfitLossReportDTO>();
        }
        catch (Exception ex)
        {
            Report = new List<ProfitLossReportDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}
