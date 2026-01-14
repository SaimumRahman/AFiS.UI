using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Suppliers
{
    public partial class SuppliersListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<SupplierModelDTO> SuppliersGrid = default!;
        protected IEnumerable<SupplierModelDTO> SuppliersList = new List<SupplierModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadSuppliers();
        }

        protected async Task LoadSuppliers()
        {
            try
            {
                IsLoading = true;
                SuppliersList = await _serviceUnitOfWork.SupplierService.GetSuppliers();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load suppliers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddSupplier()
        {
            NavigationManager.NavigateTo("/SuppliersAdd");
        }

        protected void EditSupplier(SupplierModelDTO supplier)
        {
            NavigationManager.NavigateTo($"/SuppliersAdd/{supplier.Id}");
        }

        protected async Task DeleteSupplier(SupplierModelDTO supplier)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Supplier '{supplier.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.SupplierService.DeleteSupplier(supplier.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadSuppliers();
            }
        }

        public void Dispose()
        {
            SuppliersGrid?.Dispose();
        }
    }
}
