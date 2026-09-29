using JM.UI.Entities.Model.Suppliers;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.Suppliers
{
    public partial class SuppliersAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected SupplierModelDTO Supplier { get; set; } = new();
        
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Supplier" : "Add Supplier";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();

            if (IsEditMode)
            {
                await LoadSupplier();
            }
        }

        private async Task LoadSupplier()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.SupplierService.GetSupplierById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Supplier not found.");
                    NavigationManager.NavigateTo("/SuppliersList");
                    return;
                }

                Supplier = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load supplier: {ex.Message}");
                NavigationManager.NavigateTo("/SuppliersList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected async Task Save()
        {
            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.SupplierService.SaveUpdateSupplier(Supplier);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Supplier updated successfully!" : "Supplier saved successfully!");
                    NavigationManager.NavigateTo("/SuppliersList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save supplier: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/SuppliersList");
        }
    }
}
