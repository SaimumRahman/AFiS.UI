// CompanyListComponent.razor.cs
using JM.UI.Entities.Model.Bank;
using JM.UI.Entities.Model.Company;
using JM.UI.Entities.Model.Designations;
using JM.UI.Entities.Model.Employees;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Employees
{
    public partial class EmployeeAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected EmployeeModelDTO Employee { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Employee" : "Add New Employee";
        protected string PageIcon => IsEditMode ? "edit" : "person_add";

        // Dropdown Lists
        protected List<BanksDTO> Banks { get; set; } = new();
        protected List<StoreDTO> Stores { get; set; } = new();
        protected List<DesignationDTO> Designations { get; set; } = new();
     //   protected List<ShiftModelDTO> Shifts { get; set; } = new();

        // Gender Options
        protected List<string> GenderOptions { get; set; } = new() { "Male", "Female", "Other" };

        // Blood Group Options
        protected List<string> BloodGroupOptions { get; set; } = new() { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

        // Marital Status Options
        protected List<string> MaritalStatusOptions { get; set; } = new() { "Single", "Married", "Divorced", "Widowed" };

        // Religion Options
        protected List<string> ReligionOptions { get; set; } = new() { "Islam", "Hinduism", "Buddhism", "Christianity", "Other" };

        // Status Options
        protected List<StatusOption> StatusOptions { get; set; } = new()
        {
            new StatusOption { Value = 1, Text = "Active" },
            new StatusOption { Value = 0, Text = "Inactive" },
            new StatusOption { Value = 2, Text = "On Leave" },
            new StatusOption { Value = 3, Text = "Terminated" }
        };

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadDropdowns();

            if (IsEditMode)
            {
                await LoadEmployee();
            }
            else
            {
                InitializeEmployee();
            }
        }

        private async Task LoadDropdowns()
        {
            try
            {
                // Load Banks
                var banksTask = _serviceUnitOfWork.BanksService.GetBankss();
                // Load Stores
                var storesTask = _serviceUnitOfWork.StoreService.GetStores();
             //   // Load Designations
               var designationsTask = _serviceUnitOfWork.DesignationService.GetDesignations();
                // Load Shifts
            //    var shiftsTask = _serviceUnitOfWork.ShiftService.GetShifts();

                //await Task.WhenAll(banksTask, storesTask, designationsTask, shiftsTask);

                Banks = (await banksTask).ToList();
                Stores = (await storesTask).ToList();
                Designations = (await designationsTask).ToList();
                //Shifts = (await shiftsTask).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load dropdown data: {ex.Message}");
            }
        }

        private async Task LoadEmployee()
        {
            try
            {
                IsLoading = true;
                var employee = await _serviceUnitOfWork.EmployeeService.GetEmployeeById(Id!.Value);

                if (employee == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Employee not found.");
                    NavigationManager.NavigateTo("/EmployeeList");
                    return;
                }

                Employee = employee;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load employee: {ex.Message}");
                NavigationManager.NavigateTo("/EmployeeList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeEmployee()
        {
            Employee = _serviceUnitOfWork.EmployeeService.CreateNewEmployee();
        }

        protected async Task Save()
        {
            var userObj = await sessionStorage.GetAsync<string>("UserId");
            int userId = 0;

            if (!string.IsNullOrEmpty(userObj.Value))
            {
                int.TryParse(userObj.Value, out userId);
            }

            if (IsEditMode)
            {
                Employee.ModifiedBy = userId.ToString();
            }
            else
            {
                Employee.CreatedBy = userId.ToString();
            }

            var validation = await _serviceUnitOfWork.EmployeeService.ValidateEmployee(Employee);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.EmployeeService.SaveUpdateEmployee(Employee);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Employee updated successfully!" : "Employee created successfully!");
                    NavigationManager.NavigateTo("/EmployeeList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save employee: {ex.Message}");
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
            int userId = 0;

            if (!string.IsNullOrEmpty(userObj.Value))
            {
                int.TryParse(userObj.Value, out userId);
            }

            Employee.CreatedBy = userId.ToString();

            var validation = await _serviceUnitOfWork.EmployeeService.ValidateEmployee(Employee);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.EmployeeService.SaveUpdateEmployee(Employee);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Employee created successfully!");
                    InitializeEmployee();
                    StateHasChanged();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save employee: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/EmployeeList");
        }

        protected async Task Reset()
        {
            if (IsEditMode)
            {
                await LoadEmployee();
            }
            else
            {
                InitializeEmployee();
            }
            StateHasChanged();
        }

        protected void CopyPresentToPermanent()
        {
            Employee.PermanentAddress = Employee.PresentAddress;
            StateHasChanged();
        }
    }

    public class StatusOption
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}