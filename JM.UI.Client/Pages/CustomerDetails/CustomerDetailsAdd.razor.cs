using JM.UI.Entities.Model.Accounts;
using JM.UI.Entities.Model.CustomerDetails;
using JM.UI.Entities.Model.MembershipType;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.CustomerDetails;

public partial class CustomerDetailsAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CustomerDetailsDTO Customer { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Customer" : "Add New Customer";
    protected string PageIcon => IsEditMode ? "edit" : "person_add";
    protected List<MembershipTypeDTO> MembershipTypes { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        NavigationGuard.IsGuardActive = true;
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadCustomer();
        }
        else
        {
            await InitializeCustomer();
        }
    }

    private async Task LoadCustomer()
    {
        try
        {
            IsLoading = true;
            var customer = await _serviceUnitOfWork.CustomerDetailsService.GetCustomerById(Id!.Value);
            MembershipTypes = (await _serviceUnitOfWork.MembershipTypeService.GetAll()).ToList();

            if (customer == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Customer not found.");
                NavigationManager.NavigateTo("/CustomerDetailsList");
                return;
            }

            Customer = customer;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load customer: {ex.Message}");
            NavigationManager.NavigateTo("/CustomerDetailsList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task InitializeCustomer()
    {
        Customer =  _serviceUnitOfWork.CustomerDetailsService.CreateNew();
        MembershipTypes = (await _serviceUnitOfWork.MembershipTypeService.GetAll()).ToList();
    }

    protected async Task Save()
    {
        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int? userId = null;
        if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        var validation = await _serviceUnitOfWork.CustomerDetailsService.Validate(Customer);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CustomerDetailsService.InsertUpdateCustomer(Customer);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Customer updated successfully!" : "Customer created successfully!");
                NavigationManager.NavigateTo("/CustomerDetailsList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                if (result.StatusCode == StatusCodes.Status304NotModified)
                {
                    var confirmed = await dialogService.Confirm(
                        "Do you want to create a new Customer?",
                        "Duplicate",
                        new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" }
                    );

                    if (confirmed == true)
                    {
                        // Handle new customer creation
                        Customer.IsForceAdd = true;
                        var results = await _serviceUnitOfWork.CustomerDetailsService.InsertUpdateCustomer(Customer);
                    }
                }
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

        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int? userId = null;
        if (!string.IsNullOrEmpty(userObj.Value) && int.TryParse(userObj.Value, out int parsedUserId))
        {
            userId = parsedUserId;
        }

        var validation = await _serviceUnitOfWork.CustomerDetailsService.Validate(Customer);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CustomerDetailsService.InsertUpdateCustomer(Customer);

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
        NavigationManager.NavigateTo("/CustomerDetailsList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadCustomer();
        }
        else
        {
            await InitializeCustomer();
        }
        StateHasChanged();
    }
    public void Dispose()
    {
        // Deactivate when leaving the page
        NavigationGuard.IsGuardActive = false;
    }
}
