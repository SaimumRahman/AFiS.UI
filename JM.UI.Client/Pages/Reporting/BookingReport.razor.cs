using JM.UI.Entities.Model.Reporting_D;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.Reports;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Reporting;

public partial class BookingReportComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public BookingReportService BookingReportService { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    protected List<BookingReportDTO> Bookings { get; set; } = new();
    protected List<StoreDTO> Stores { get; set; } = new();
    protected int? SelectedStoreId { get; set; }
    protected DateTime? FromDate { get; set; }
    protected DateTime? ToDate { get; set; }
    protected bool IsLoading { get; set; }
    protected bool IsPrinting { get; set; }

    protected string DateRangeText =>
        (FromDate.HasValue && ToDate.HasValue)
            ? $"DATE RANGE: {FromDate:dd-MMM-yy} to {ToDate:dd-MMM-yy}"
            : "DATE RANGE: ALL TIME";

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
            Bookings = (await _serviceUnitOfWork.ReportingService.GetBookingReport(SelectedStoreId, FromDate, ToDate))?.ToList() ?? new List<BookingReportDTO>();
        }
        catch (Exception ex)
        {
            Bookings = new List<BookingReportDTO>();
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
        if (!Bookings.Any())
        {
            notificationService.Notify(NotificationSeverity.Warning, "No Data", "There are no booked invoices to print.");
            return;
        }

        try
        {
            IsPrinting = true;
            StateHasChanged();

            var pdfBytes = BookingReportService.GenerateBookingReport(
                Bookings,
                dateFrom: FromDate,
                dateTo: ToDate);

            var fileName = $"BookingReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            await JS.InvokeVoidAsync("downloadFileFromBytes", fileName, "application/pdf", pdfBytes);

            notificationService.Notify(NotificationSeverity.Success, "Success", $"Booking report generated — {Bookings.Count} line(s).");
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