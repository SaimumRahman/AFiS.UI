using JM.UI.Entities.Model.SupplierPayments;
using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.SupplierPayments
{
    public partial class SupplierLedgerComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected IEnumerable<SupplierModelDTO> SuppliersList = new List<SupplierModelDTO>();
        protected IEnumerable<SupplierLedgerDTO> LedgerList = new List<SupplierLedgerDTO>();
        protected int SelectedSupplierId;
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadSuppliers();
        }

        protected async Task LoadSuppliers()
        {
            SuppliersList = await _serviceUnitOfWork.SupplierService.GetSuppliers();
        }

        protected async Task OnSupplierChange(object value)
        {
            if (SelectedSupplierId != 0)
            {
                await LoadLedger();
            }
            else
            {
                LedgerList = new List<SupplierLedgerDTO>();
            }
        }

        protected async Task LoadLedger()
        {
            try
            {
                IsLoading = true;
                var data = await _serviceUnitOfWork.SupplierPaymentService.GetSupplierLedger(SelectedSupplierId);
                
                // Calculate Running Balance
                decimal balance = 0;
                var calculatedList = new List<SupplierLedgerDTO>();
                
                // Assuming data is sorted by Date. If not, sort it.
                foreach (var item in data.OrderBy(x => x.TransactionDate).ThenBy(x => x.TransactionId))
                {
                    // Credit increases balance (we owe more), Debit decreases balance (we paid)
                    // Assuming Supplier Ledger: Credit = Purchase, Debit = Payment
                    balance += item.CreditAmount - item.DebitAmount;
                    item.RunningBalance = balance;
                    calculatedList.Add(item);
                }
                
                LedgerList = calculatedList; // calculatedList.OrderByDescending(x => x.TransactionDate); // If we want to show latest first reversed
            }
            catch (Exception ex)
            {
                 notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load ledger: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void PrintLedger()
        {
            notificationService.Notify(NotificationSeverity.Info, "Info", "Print functionality to be implemented");
        }
    }
}
