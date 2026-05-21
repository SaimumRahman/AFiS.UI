// CompanyListComponent.razor.cs
using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.Employees;
using JM.UI.Entities.Model.Routes;
using JM.UI.Service.Routes;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.Users;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Employees
{
    public partial class EmployeeListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public IUserAuthService _userAuthService { get; set; } = default!;

        protected RadzenDataGrid<EmployeeModelDTO> EmployeesGrid = default!;
        protected IEnumerable<EmployeeModelDTO> Employees = new List<EmployeeModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                IsLoading = true;
                Employees = await _serviceUnitOfWork
                    .EmployeeService
                    .GetEmployees();
            }
            catch (Exception ex)
            {
                notificationService.Notify(
                    NotificationSeverity.Error,
                    "Error",
                    $"Failed to load employees: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddEmployee()
        {
            NavigationManager.NavigateTo("/EmployeeAdd");
        }

        protected void EditEmployee(EmployeeModelDTO employee)
        {
            NavigationManager.NavigateTo($"/EmployeeAdd/{employee.Id}");
        }

        protected async Task DeleteEmployee(EmployeeModelDTO employee)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete '{employee.Name}'?",
                "Confirm Delete"
            );

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork
                    .EmployeeService
                    .DeleteEmployee(employee.Id);

                notificationService.Notify(
                    result.IsSuccessStatus
                        ? NotificationSeverity.Success
                        : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error",
                    result.Message
                );

                if (result.IsSuccessStatus)
                    await LoadEmployees();
            }
        }
        protected async Task ToggleStatus(EmployeeModelDTO employee)
        {
            try
            {
                if (employee.Id <= 0)
                {
                    notificationService.Notify(new NotificationMessage   // ← lowercase
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No User Account",
                        Detail = $"'{employee.Name}' has no linked user account.",
                        Duration = 4000
                    });
                    return;
                }

                var confirmResult = await dialogService.Confirm(
                    $"Are you sure you want to {(employee.IsActive ? "deactivate" : "activate")} '{employee.Surname}'?",
                    "Confirm Status Change",
                    new ConfirmOptions
                    {
                        OkButtonText = "Yes",
                        CancelButtonText = "No",
                        AutoFocusFirstElement = true
                    }
                );

                if (confirmResult != true) return;

                bool newStatus = !employee.IsActive;

                var success = await _userAuthService.UpdateActiveInactiveUser(employee.Surname, newStatus);

                if (success)
                {
                    notificationService.Notify(new NotificationMessage   // ← lowercase
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = $"'{employee.Name}' has been {(newStatus ? "activated" : "deactivated")} successfully.",
                        Duration = 4000
                    });

                    await LoadEmployees();
                    await EmployeesGrid.Reload();
                }
                else
                {
                    notificationService.Notify(new NotificationMessage   // ← lowercase
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Failed to update employee status.",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(new NotificationMessage       // ← lowercase
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"An error occurred: {ex.Message}",
                    Duration = 4000
                });
            }
        }
        protected string GetStatusText(int status) =>
            _serviceUnitOfWork.EmployeeService.GetStatusText(status);

        protected void ShowTooltip(ElementReference element, string text)
        {
            TooltipService.Open(element, text);
        }

        public void Dispose()
        {
            EmployeesGrid?.Dispose();
        }
    }
}