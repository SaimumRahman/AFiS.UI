using JM.UI.Entities.Model.Customer;
using JM.UI.Service.Customer;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Dialog
{
    public partial class CustomerDetailsDialogComponent : PosComponentBase
    {
        [Inject] public ICustomerService CustomerService { get; set; } = default!;

        [Parameter] public CustomerModelDTO Customer { get; set; } = new();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsEditMode => Customer.CustomerID > 0;

        protected List<string> CustomerTypes { get; set; } = new()
        {
            "Individual",
            "Corporate",
            "Retailer",
            "Wholesaler"
        };

        protected override void OnInitialized()
        {
            if (string.IsNullOrEmpty(Customer.CustomerType))
            {
                Customer.CustomerType = "Individual";
            }
        }

        protected async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Customer.CustomerName))
            {
                //notificationService.Notify(NotificationSeverity.Warning, "Validation", "Customer name is required.");
                return;
            }

            try
            {
                IsProcessing = true;

                if (!IsEditMode)
                {
                    Customer.CreatedDate = DateTime.Now;
                }

                await CustomerService.SaveUpdateCustomer(Customer);

                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Customer updated successfully!" : "Customer created successfully!");

                dialogService.Close(true);
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save customer: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            dialogService.Close(false);
        }
    }
}