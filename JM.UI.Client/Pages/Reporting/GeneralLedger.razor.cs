using JM.UI.Entities.Model.Reporting_D;
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

public partial class AccountingGeneralLedgerComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected RadzenDataGrid<GeneralLedgerDTO> ReportGrid = default!;
    protected List<GeneralLedgerDTO> Rows { get; set; } = new();
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected string? SelectedBranchCode { get; set; }
    protected bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        FromDate = AccountingReportOptions.CurrentFiscalYearStart();
        ToDate = DateTime.Today;
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetGeneralLedger(null, FromDate, ToDate, SelectedBranchCode, null, null))?.ToList() ?? new List<GeneralLedgerDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<GeneralLedgerDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}