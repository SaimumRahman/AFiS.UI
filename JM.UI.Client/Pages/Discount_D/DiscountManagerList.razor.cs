using JM.UI.Entities.Model.Discount_D;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Discount_D
{
    public partial class DiscountManagerListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<DiscountManagerDTO> CampaignsGrid = default!;
        protected IEnumerable<DiscountManagerDTO> Campaigns { get; set; } = new List<DiscountManagerDTO>();
        protected bool IsLoading { get; set; } = false;
        protected HashSet<int> LoadingDetails { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCampaigns();
        }

        private async Task LoadCampaigns()
        {
            try
            {
                IsLoading = true;
                Campaigns = await _serviceUnitOfWork.DiscountManagerService.GetAll();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load campaigns: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OnRowExpand(DiscountManagerDTO campaign)
        {
            if (campaign.DiscountDetails?.Count > 0) return;

            try
            {
                LoadingDetails.Add(campaign.Id);
                StateHasChanged();

                var full = await _serviceUnitOfWork.DiscountManagerService.GetById(campaign.Id);
                if (full != null)
                {
                    campaign.DiscountDetails = full.DiscountDetails;
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load details: {ex.Message}");
            }
            finally
            {
                LoadingDetails.Remove(campaign.Id);
                StateHasChanged();
            }
        }

        protected void AddCampaign()
        {
            NavigationManager.NavigateTo("/DiscountManagerAdd");
        }

        protected void EditCampaign(DiscountManagerDTO campaign)
        {
            NavigationManager.NavigateTo($"/DiscountManagerAdd/{campaign.Id}");
        }

        protected async Task DeleteCampaign(DiscountManagerDTO campaign)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete campaign '{campaign.DiscountName}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.DiscountManagerService.Delete(campaign.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Campaign deleted successfully.");
                    await LoadCampaigns();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete campaign.");
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
            CampaignsGrid?.Dispose();
        }
    }
}
