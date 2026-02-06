using JM.UI.Entities.Model.PurchaseItems;
using JM.UI.Entities.Model.Purchases;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

public partial class PurchaseListComponent : PosComponentBase, IDisposable
{
    [Inject] public IServiceUnitOfWork ServiceUnitOfWork { get; set; } = default!;
    [Inject] public NotificationService NotificationService { get; set; } = default!;
    [Inject] public DialogService DialogService { get; set; } = default!;

    protected RadzenDataGrid<PurchaseSummaryDTO>? PurchasesGrid;
    protected IEnumerable<PurchaseSummaryDTO> Purchases = new List<PurchaseSummaryDTO>();
    protected Dictionary<int, List<PurchaseItemDTO>> PurchaseItemsCache = new();
    protected bool IsLoading;

    protected override async Task OnInitializedAsync()
    {
        await LoadPurchases();
    }

    protected async Task LoadPurchases()
    {
        try
        {
            IsLoading = true;
            Purchases = await ServiceUnitOfWork.PurchaseService.GetAllPurchases();
        }
        catch (Exception ex)
        {
            NotifyError($"Failed to load purchases: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected List<PurchaseItemDTO> GetPurchaseItems(PurchaseSummaryDTO purchase)
    {
        // Return cached items or empty list
        return PurchaseItemsCache.GetValueOrDefault(purchase.Id) ?? new List<PurchaseItemDTO>();
    }

    protected async Task OnRowExpand(PurchaseSummaryDTO purchase)
    {
        // Skip if already loaded
        if (PurchaseItemsCache.ContainsKey(purchase.Id))
            return;

        try
        {
            var items = await ServiceUnitOfWork.PurchaseService.GetPurchaseItems(purchase.Id);
            PurchaseItemsCache[purchase.Id] = items?.ToList() ?? new List<PurchaseItemDTO>();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            NotifyError($"Failed to load purchase items: {ex.Message}");
            // Add empty list to prevent repeated failed attempts
            PurchaseItemsCache[purchase.Id] = new List<PurchaseItemDTO>();
        }
    }

    protected void AddPurchase()
    {
        NavigationManager.NavigateTo("/PurchaseEntry");
    }

    protected void EditPurchase(PurchaseSummaryDTO purchase)
    {
        NavigationManager.NavigateTo($"/PurchaseEntry/Edit/{purchase.Id}");
    }

    protected async Task DeletePurchase(PurchaseSummaryDTO purchase)
    {
        var confirm = await DialogService.Confirm(
            $"Are you sure you want to delete purchase '{purchase.BillInvoiceNumber}'?",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

        if (confirm == true)
        {
            try
            {
                await ServiceUnitOfWork.PurchaseService.DeletePurchase(purchase.Id);
                PurchaseItemsCache.Remove(purchase.Id);
                await LoadPurchases();
                NotifySuccess("Purchase deleted successfully");
            }
            catch (Exception ex)
            {
                NotifyError($"Failed to delete purchase: {ex.Message}");
            }
        }
    }

    private void NotifyError(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000
        });
    }

    private void NotifySuccess(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 3000
        });
    }

    public void Dispose()
    {
        PurchasesGrid?.Dispose();
        PurchaseItemsCache.Clear();
    }
}