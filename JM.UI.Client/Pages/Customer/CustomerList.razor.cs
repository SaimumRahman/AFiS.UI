using JM.UI.Entities.Model.Customer;
using JM.UI.Service.Customer;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Customer
{
    public partial class CustomerListComponent : PosComponentBase
    {
        [Inject] public ICustomerService CustomerService { get; set; } = default!;

        protected RadzenDataGrid<CustomerModelDTO> CustomersGrid = default!;
        protected IEnumerable<CustomerModelDTO> Customers { get; set; } = new List<CustomerModelDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            try
            {
                IsLoading = true;
                Customers = await CustomerService.GetCustomers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load Customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void AddCustomer()
        {
            NavigationManager.NavigateTo("/CustomerAdd");
        }

        protected void EditCustomer(CustomerModelDTO customer)
        {
            NavigationManager.NavigateTo($"/CustomerAdd/{customer.CustomerID}");
        }

        protected void ViewCustomer(CustomerModelDTO customer)
        {
            NavigationManager.NavigateTo($"/CustomerAdd/{customer.CustomerID}");
        }

        protected async Task DeleteCustomer(CustomerModelDTO customer)
        {
            var confirmResult = await dialogService.Confirm(
                $"Are you sure you want to delete customer '{customer.CustomerName}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirmResult == true)
            {
                var result = await CustomerService.DeleteCustomer(customer.CustomerID);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message);
                    await LoadCustomers();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
        }

        protected async Task ToggleStatus(CustomerModelDTO customer)
        {
            var userObj = await sessionStorage.GetAsync<string>("UserId");
            customer.CreatedBy = Convert.ToInt32(userObj.Value);
            var result = await CustomerService.ToggleCustomerStatus(customer.CustomerID);
            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", result.Message);
                await LoadCustomers();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }

        protected string Truncate(string? value, int maxChars) => CustomerService.Truncate(value, maxChars);

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            CustomersGrid?.Dispose();
        }
    }
}