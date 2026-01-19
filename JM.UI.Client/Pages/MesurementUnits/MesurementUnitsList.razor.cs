using JM.UI.Entities.Model.MesurementUnits;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JM.UI.Client.Pages.MesurementUnits
{
    public partial class MesurementUnitsListComponent : PosComponentBase, IDisposable
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<MesurementUnitModelDTO> MesurementUnitsGrid = default!;
        protected IEnumerable<MesurementUnitModelDTO> MesurementUnitsList = new List<MesurementUnitModelDTO>();
        protected bool IsLoading;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadMesurementUnits();
        }

        protected async Task LoadMesurementUnits()
        {
            try
            {
                IsLoading = true;
                MesurementUnitsList = await _serviceUnitOfWork.MesurementUnitService.GetMesurementUnits();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load measurement units: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddMesurementUnit()
        {
            NavigationManager.NavigateTo("/MesurementUnitsAdd");
        }

        protected void EditMesurementUnit(MesurementUnitModelDTO unit)
        {
            NavigationManager.NavigateTo($"/MesurementUnitsAdd/{unit.Id}");
        }

        protected async Task DeleteMesurementUnit(MesurementUnitModelDTO unit)
        {
            var confirm = await dialogService.Confirm($"Are you sure you want to delete Measurement Unit '{unit.Name}'?", "Confirm Delete");

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.MesurementUnitService.DeleteMesurementUnit(unit.Id);

                notificationService.Notify(result.IsSuccessStatus ? NotificationSeverity.Success : NotificationSeverity.Error,
                    result.IsSuccessStatus ? "Success" : "Error", result.Message);

                if (result.IsSuccessStatus)
                    await LoadMesurementUnits();
            }
        }

        public void Dispose()
        {
            MesurementUnitsGrid?.Dispose();
        }
    }
}
