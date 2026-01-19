using JM.UI.Entities.Model.Items;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Items
{
    public partial class ItemsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<ItemModelDTO> ItemsGrid = default!;
        protected IEnumerable<ItemModelDTO> ItemsList = new List<ItemModelDTO>();
        protected bool IsLoading;

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

        protected void AddItem()
        {
            NavigationManager.NavigateTo("/ItemsAdd");
        }

        protected void EditItem(ItemModelDTO item)
        {
            NavigationManager.NavigateTo($"/ItemsAdd/{item.Id}");
        }

        protected async Task DeleteItem(ItemModelDTO item)
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
