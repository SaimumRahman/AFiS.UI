using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Service.UnitOfWork;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Dialog.SalesPOS
{
    public partial class AddCustomerDialogComponent : ComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService DialogService { get; set; } = default!;
        [Inject] public NotificationService NotificationService { get; set; } = default!;

        protected CustomerDetailsDTO NewCustomer { get; set; } = new() { IsForceAdd = true };

        protected async Task SaveNewCustomer()
        {
            if (string.IsNullOrWhiteSpace(NewCustomer.Name) || string.IsNullOrWhiteSpace(NewCustomer.Phone))
            {
                NotificationService.Notify(NotificationSeverity.Warning, "Validation",
                    "Name and Phone are required", 3000);
                return;
            }

            var result = await _serviceUnitOfWork.CustomerDetailsService.InsertUpdateCustomer(NewCustomer);
            if (result.IsSuccessStatus)
            {
                var all = await _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers();
                var saved = all.OrderByDescending(c => c.Id).FirstOrDefault();
                DialogService.Close(saved);
            }
            else
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", result.Message, 4000);
            }
        }

        protected void Cancel()
        {
            DialogService.Close(null);
        }
    }
}
