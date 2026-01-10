// CompanyListComponent.razor.cs
using JM.UI.Entities.Model.Company;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Company
{
    public partial class CompanyListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CompanyDTO> CompaniesGrid = default!;
        protected IEnumerable<CompanyDTO> Companies { get; set; } = new List<CompanyDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCompanies();
        }

        private async Task LoadCompanies()
        {
            try
            {
                IsLoading = true;
                Companies = await _serviceUnitOfWork.CompanyService.GetCompanies();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load companies: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddCompany()
        {
            NavigationManager.NavigateTo("/CompanyAdd");
        }

        protected void EditCompany(CompanyDTO company)
        {
            NavigationManager.NavigateTo($"/CompanyAdd/{company.Id}");
        }

        protected async Task DeleteCompany(CompanyDTO company)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete company '{company.Name}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.CompanyService.DeleteCompany(company.Id);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Company deleted successfully.");
                    await LoadCompanies();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete company.");
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
            CompaniesGrid?.Dispose();
        }
    }
}