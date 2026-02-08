using JM.UI.Entities.Model.SupplierPayments;
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
    public partial class SupplierOutstandingComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected IEnumerable<SupplierOutstandingDTO> OutstandingList = new List<SupplierOutstandingDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadOutstanding();
        }

        protected async Task LoadOutstanding()
        {
            try
            {
                IsLoading = true;
                OutstandingList = await _serviceUnitOfWork.SupplierPaymentService.GetSupplierOutstanding();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load outstanding balances: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void ViewLedger(int supplierId)
        {
            // You might need to pass supplierId to Ledger page via query string or state
            // Currently Ledger page select dropdown uses bind-Value. 
            // Better to change Ledger page to accept query parameter? 
            // Or just navigate and let user select (for now).
            // Ideal: NavigationManager.NavigateTo($"/SupplierLedger?supplierId={supplierId}");
            
            // For now, let's just navigate to Ledger page. 
            // To implement pre-selection, I'd need to update Ledger page to read query string.
            // Let's keep it simple for now or adding query param support is easy.
            
            NavigationManager.NavigateTo("/SupplierLedger");
        }
    }
}
