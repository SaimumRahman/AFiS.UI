using JM.UI.Entities.Model.PurchaseReturnItems;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.PurchaseReturns
{
    public class PurchaseReturnItemListBase : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected IEnumerable<PurchaseReturnItemModelDTO> ReturnItems { get; set; } = new List<PurchaseReturnItemModelDTO>();
        protected RadzenDataGrid<PurchaseReturnItemModelDTO> ItemsGrid;
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadData();
        }

        protected async Task LoadData()
        {
            try
            {
                IsLoading = true;
                // We'll add a global "GetAllReturnItems" to the service if needed, 
                // for now we'll fetch them through the service.
                var result = await _serviceUnitOfWork.PurchaseReturnItemService.GetAllReturnItems();
                ReturnItems = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load return items: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
