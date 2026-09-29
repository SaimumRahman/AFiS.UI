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

public partial class AccountingPartyBalanceComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    protected List<PartyBalanceDTO> Rows { get; set; } = new();
    protected string? SelectedPartyType { get; set; }
    protected bool IsLoading { get; set; }

    protected string TypeRangeText =>
        string.IsNullOrWhiteSpace(SelectedPartyType)
            ? "ALL PARTY TYPES"
            : $"PARTY TYPE: {SelectedPartyType}";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            Rows = (await _serviceUnitOfWork.ReportingService.GetPartyBalance(null, SelectedPartyType))?.ToList() ?? new List<PartyBalanceDTO>();
        }
        catch (Exception ex)
        {
            Rows = new List<PartyBalanceDTO>();
            notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
}