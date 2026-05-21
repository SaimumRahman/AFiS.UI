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
        protected async Task ToggleStatus(EmployeeModelDTO employee, bool value)
        {
            var result = await _userAuthService.UpdateActiveInactiveUser(employee.Surname, value);

            if (result)
            {
                notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = $"Status updated successfully.",
                    Duration = 3000
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