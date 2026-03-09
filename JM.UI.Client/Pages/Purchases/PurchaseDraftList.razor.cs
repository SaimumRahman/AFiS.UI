using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Purchases
{
    // FIX: explicitly implement IDisposable
    public partial class PurchaseDraftListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected IEnumerable<PurchaseDraftDTO> Drafts { get; set; } = new List<PurchaseDraftDTO>();
        protected RadzenDataGrid<PurchaseDraftDTO> DraftsGrid = default!;
        protected bool IsLoading { get; set; } = false;
        protected bool IsDeleting { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadDrafts();
        }

        protected async Task LoadDrafts()
        {
            try
            {
                IsLoading = true;
                Drafts = await _serviceUnitOfWork.PurchaseService.GetPurchaseDrafts()
                         ?? new List<PurchaseDraftDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to load drafts: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                // FIX: ensure UI refreshes after data is assigned
                StateHasChanged();
            }
        }

        protected void CreateNewDraft()
        {
            NavigationManager.NavigateTo("/PurchaseEntry");
        }

        protected void LoadDraft(int draftId)
        {
            NavigationManager.NavigateTo($"/PurchaseEntry/Draft/{draftId}");
        }

        protected async Task DeleteDraft(int draftId)
        {
            try
            {
                var confirmed = await dialogService.Confirm(
                    "Are you sure you want to delete this draft?",
                    "Delete Draft",
                    new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

                if (confirmed != true) return;

                IsDeleting = true;

                var result = await _serviceUnitOfWork.PurchaseService.DeletePurchaseDraft(draftId);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        result.Message ?? "Draft deleted successfully");

                    // FIX: reload data first, then reload grid — no duplicate reload
                    await LoadDrafts();
                    await DraftsGrid.Reload();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error",
                        result.Message ?? "Failed to delete draft");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    $"Failed to delete draft: {ex.Message}");
            }
            finally
            {
                IsDeleting = false;
                StateHasChanged();
            }
        }

        protected string FormatCurrency(decimal amount) =>
            _serviceUnitOfWork.PurchaseService.FormatCurrency(amount);

        protected string FormatDate(DateTime? date) =>
            _serviceUnitOfWork.PurchaseService.FormatDate(date);

        protected string GetItemCount(PurchaseDraftDTO draft) =>
            draft.DraftItems?.Count.ToString() ?? "0";

        public void Dispose() => DraftsGrid?.Dispose();
    }
}