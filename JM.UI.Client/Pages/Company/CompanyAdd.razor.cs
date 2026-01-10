using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.Model.Company;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Company;

public partial class CompanyAddComponent : PosComponentBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected CompanyDTO Company { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Company" : "Add New Company";
    protected string PageIcon => IsEditMode ? "edit" : "business";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadCompany();
        }
        else
        {
            InitializeCompany();
        }
    }

    private async Task LoadCompany()
    {
        try
        {
            IsLoading = true;
            var company = await _serviceUnitOfWork.CompanyService.GetCompanyById(Id!.Value);

            if (company == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Company not found.");
                NavigationManager.NavigateTo("/CompanyList");
                return;
            }

            Company = company;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load company: {ex.Message}");
            NavigationManager.NavigateTo("/CompanyList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeCompany()
    {
        Company = _serviceUnitOfWork.CompanyService.CreateNewCompany();
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
            Company.ModifiedBy = userId.ToString();
        }
        else
        {
            Company.CreatedBy = userId.ToString();
        }

        var validation = await _serviceUnitOfWork.CompanyService.ValidateCompany(Company);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CompanyService.SaveUpdateCompany(Company);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Company updated successfully!" : "Company created successfully!");
                NavigationManager.NavigateTo("/CompanyList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save company: {ex.Message}");
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

        Company.CreatedBy = userId.ToString();

        var validation = await _serviceUnitOfWork.CompanyService.ValidateCompany(Company);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.CompanyService.SaveUpdateCompany(Company);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Company created successfully!");
                InitializeCompany();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save company: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/CompanyList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadCompany();
        }
        else
        {
            InitializeCompany();
        }
        StateHasChanged();
    }
}