using JM.UI.Entities.Model.Items;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace JM.UI.Client.Pages.Items
{
    public partial class ItemsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ItemDTO> ItemsGrid = default!;
        protected IEnumerable<ItemDTO> ItemsList = new List<ItemDTO>();
        protected bool IsLoading;

        /// <summary>Free-text filter: partial barcode OR partial item name.</summary>
        protected string? SearchText { get; set; }

        /// <summary>Grid data after the free-text filter is applied.</summary>
        protected IEnumerable<ItemDTO> FilteredItems
        {
            get
            {
                var term = SearchText?.Trim();
                if (string.IsNullOrEmpty(term)) return ItemsList;

                return ItemsList.Where(i =>
                    (i.Barcode ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (i.Name ?? string.Empty).Contains(term, StringComparison.OrdinalIgnoreCase));
            }
        }

        protected int FilteredItemCount => FilteredItems.Count();

        protected void ClearSearch() => SearchText = null;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadItems();
        }

        protected async Task LoadItems()
        {
            try
            {
                IsLoading = true;
                ItemsList = await _serviceUnitOfWork.ItemService.GetItems();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load items: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void EditItem(ItemDTO item)
        {
            NavigationManager.NavigateTo($"/ItemsAdd/{item.Id}");
        }

        protected async Task DeleteItem(ItemDTO item)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Item '{item.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.ItemService.DeleteItem(item.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadItems();
            }
        }

        public void Dispose()
        {
            ItemsGrid?.Dispose();
        }
    }
}
