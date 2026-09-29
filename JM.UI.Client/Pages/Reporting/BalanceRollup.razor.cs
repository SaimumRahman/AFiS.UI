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

public partial class AccountingBalanceRollupComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected List<BalanceRollupDTO> Rows { get; set; } = new();
    protected string PeriodText { get; set; } = string.Empty;
    protected bool IsLoading { get; set; }

    protected string PeriodRangeText =>
        string.IsNullOrWhiteSpace(PeriodText)
            ? "ALL PERIODS"
            : $"PERIOD: {PeriodText}";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        int? periodId = null;
        if (int.TryParse(PeriodText?.Trim(), out var parsed) && parsed > 0)
            periodId = parsed;

        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetBalanceRollup(null, periodId))?.ToList() ?? new List<BalanceRollupDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<BalanceRollupDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}