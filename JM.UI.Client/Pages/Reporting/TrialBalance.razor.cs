using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class AccountingTrialBalanceComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected List<TrialBalanceDTO> Rows { get; set; } = new();
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected string? SelectedBranchCode { get; set; }
    protected bool IsLoading { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"DATE RANGE: {FromDate:dd-MMM-yy} to {ToDate:dd-MMM-yy}"
            : "DATE RANGE: ALL TIME";

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
            Rows = (await _serviceUnitOfWork.ReportingService.GetTrialBalance(null, FromDate, ToDate, SelectedBranchCode))?.ToList() ?? new List<TrialBalanceDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<TrialBalanceDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}