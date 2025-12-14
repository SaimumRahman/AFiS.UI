using JM.UI.Entities.Model.Customer;
using JM.UI.Service.Customer;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Customer
{
    public partial class CustomerAddComponent : PosComponentBase
    {
        [Inject] public ICustomerService CustomerService { get; set; } = default!;

        [Parameter] public int? CustomerID { get; set; }

        protected CustomerModelDTO Customer { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => CustomerID.HasValue && CustomerID.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Customer" : "Add New Customer";
        protected string PageIcon => IsEditMode ? "edit" : "person_add";
        protected List<string> CustomerTypes => CustomerService.GetCustomerTypes();

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            if (IsEditMode)
            {
                await LoadCustomer();
            }
            else
            {
                InitializeCustomer();
            }
        }

        private async Task LoadCustomer()
        {
            try
            {
                IsLoading = true;
                var customer = await CustomerService.GetCustomerById(CustomerID!.Value);

                if (customer == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Customer not found.");
                    NavigationManager.NavigateTo("/CustomerList");
                    return;
                }

                Customer = customer;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load customer: {ex.Message}");
                NavigationManager.NavigateTo("/CustomerList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeCustomer()
        {
            Customer = CustomerService.CreateNewCustomer();
        }

        protected async Task Save()
        {
            var userObj = await sessionStorage.GetAsync<string>("UserId");
            Customer.CreatedBy = Convert.ToInt32(userObj.Value);

            var validation = await CustomerService.ValidateCustomer(Customer);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await CustomerService.SaveUpdateCustomer(Customer);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Customer updated successfully!" : "Customer created successfully!");
                    NavigationManager.NavigateTo("/CustomerList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
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

        protected async Task SaveAndNew()
        {
            if (IsEditMode)
            {
                await Save();
                return;
            }

            var validation = await CustomerService.ValidateCustomer(Customer);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await CustomerService.SaveUpdateCustomer(Customer);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Customer created successfully!");
                    InitializeCustomer();
                    StateHasChanged();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
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
            NavigationManager.NavigateTo("/CustomerList");
        }

        protected async Task Reset()
        {
            if (IsEditMode)
            {
                await LoadCustomer();
            }
            else
            {
                InitializeCustomer();
            }
            StateHasChanged();
        }
    }
}