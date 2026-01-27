using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.MesurementUnits
{
    public partial class MesurementUnitsAddComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected MesurementUnitModelDTO MesurementUnit { get; set; } = new();

        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Measurement Unit" : "New Measurement Unit";

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            if (IsEditMode)
            {
                await LoadMesurementUnit();
            }
        }

        private async Task LoadMesurementUnit()
        {
            try
            {
                IsLoading = true;
                var result = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnitById(Id!.Value);

                if (result == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Measurement Unit not found.");
                    NavigationManager.NavigateTo("/MesurementUnitsList");
                    return;
                }

                MesurementUnit = result;
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load unit: {ex.Message}");
                NavigationManager.NavigateTo("/MesurementUnitsList");
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
                var result = await _serviceUnitOfWork.MesurementUnitService.SaveUpdateMesurementUnit(MesurementUnit);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Measurement Unit updated successfully!" : "Measurement Unit created successfully!");
                    NavigationManager.NavigateTo("/MesurementUnitsList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Already Exist");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/MesurementUnitsList");
        }
    }
}
