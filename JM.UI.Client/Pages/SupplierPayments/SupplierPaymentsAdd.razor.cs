using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Stores;
using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SupplierPayments
{
    public partial class SupplierPaymentsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Parameter] public int Id { get; set; }

        protected SupplierPaymentDTO PaymentModel { get; set; } = new();
        protected IEnumerable<SupplierModelDTO> SuppliersList = new List<SupplierModelDTO>();
        protected IEnumerable<StoreDTO> StoresList = new List<StoreDTO>();
        protected IEnumerable<BanksDTO> BanksList = new List<BanksDTO>();
        
        protected List<string> PaymentMethods = new List<string> { "Cash", "Bank", "Cheque" };
        protected bool IsBankVisible = false;
        protected bool IsChequeVisible = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadDropdowns();

            if (Id != 0)
            {
                var payment = await _serviceUnitOfWork.SupplierPaymentService.GetSupplierPaymentById(Id);
                if (payment != null)
                {
                    PaymentModel = payment;
                    UpdateVisibility();
                }
            }
            else
            {
                // Set defaults
                PaymentModel.PaymentDate = DateTime.Now;
                PaymentModel.PaymentMethod = "Cash";
                // You might want to fetch current user name from TokenService/AuthState
                 var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                 PaymentModel.UserName = authState.User.Identity?.Name ?? "Admin";
            }
        }

        protected async Task LoadDropdowns()
        {
            SuppliersList = await _serviceUnitOfWork.SupplierService.GetSuppliers();
            StoresList = await _serviceUnitOfWork.StoreService.GetStores();
            BanksList = await _serviceUnitOfWork.BanksService.GetBankss();
        }

        protected void OnPaymentMethodChange(object value)
        {
            UpdateVisibility();
        }

        protected void UpdateVisibility()
        {
            var method = PaymentModel.PaymentMethod;
            IsBankVisible = method == "Bank" || method == "Cheque";
            IsChequeVisible = method == "Cheque";
        }

        protected async Task SavePayment(SupplierPaymentDTO payment)
        {
            try
            {
                 // Ensure UserName is set if new
                if (payment.Id == 0 && string.IsNullOrEmpty(payment.UserName))
                {
                     var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                     payment.UserName = authState.User.Identity?.Name ?? "System";
                }

                var result = await _serviceUnitOfWork.SupplierPaymentService.SaveUpdateSupplierPayment(payment);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Payment saved successfully");
                    NavigationManager.NavigateTo("/SupplierPaymentsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save payment: {ex.Message}");
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/SupplierPaymentsList");
        }
    }
}
