using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SupplierPayments
{
    public partial class SupplierPaymentsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<SupplierPaymentDTO> PaymentsGrid = default!;
        protected IEnumerable<SupplierPaymentDTO> PaymentsList = new List<SupplierPaymentDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadPayments();
        }

        protected async Task LoadPayments()
        {
            try
            {
                IsLoading = true;
                PaymentsList = await _serviceUnitOfWork.SupplierPaymentService.GetSupplierPayments();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load payments: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddPayment()
        {
            NavigationManager.NavigateTo("/SupplierPaymentsAdd");
        }

        protected void EditPayment(SupplierPaymentDTO payment)
        {
            NavigationManager.NavigateTo($"/SupplierPaymentsAdd/{payment.Id}");
        }

        protected async Task DeletePayment(SupplierPaymentDTO payment)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Payment Ref: '{payment.ReferenceNo}' for amount {payment.PaymentAmount:C}?", "Confirm Delete");

            if (confirm == true)
            {
                try
                {
                    var result = await _serviceUnitOfWork.SupplierPaymentService.DeleteSupplierPayment(payment.Id);

                    notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                        result.IsSuccessStatus ? "Success" : "Error", result.Message);

                    if (result.IsSuccessStatus)
                        await LoadPayments();
                }
                catch (Exception ex)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to delete payment: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            PaymentsGrid?.Dispose();
        }
    }
}
