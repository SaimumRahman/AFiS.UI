using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.CustomerDetails
{
    public partial class CustomerDetailsListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CustomerDetailsDTO> CustomersGrid = default!;
        protected IEnumerable<CustomerDetailsDTO> Customers { get; set; } = new List<CustomerDetailsDTO>();
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
                Customers = await _serviceUnitOfWork.CustomerDetailsService.GetAllCustomers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load customers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddCustomer()
        {
            NavigationManager.NavigateTo("/CustomerDetailsAdd");
        }

        protected void EditCustomer(CustomerDetailsDTO customer)
        {
            NavigationManager.NavigateTo($"/CustomerDetailsAdd/{customer.Id}");
        }

        protected async Task DeleteCustomer(CustomerDetailsDTO customer)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete customer '{customer.Name}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.CustomerDetailsService.DeleteCustomer(customer.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Customer deleted successfully.");
                    await LoadCustomers();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete customer.");
                }
            }
        }

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

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
