using JM.UI.Client.Shared;
using JM.UI.Entities.Services;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Collections.Generic;
using System.Threading.Tasks;
using JM.UI.Client.Shared;
using JM.UI.Entities.Model.StockOpening;
using JM.UI.Service.UnitOfWork;

namespace JM.UI.Client.Pages.StockOpening
{
    public partial class StockOpeningListComponent : ComponentBase
    {
        public IEnumerable<StockOpeningEntryDTO> StockOpenings { get; set; } = new List<StockOpeningEntryDTO>();
        public bool IsLoading { get; set; } = true;

        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        
        protected string PageTitleOverride { get; set; } = "Stock Openings";

        protected override async Task OnInitializedAsync()
        {
            PageTitleOverride = "Stock Openings";
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                IsLoading = true;
                StateHasChanged();
                StockOpenings = await _serviceUnitOfWork.StockOpeningService.GetAllStockOpenings();
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        public void AddNewStockOpening()
        {
            NavigationManager.NavigateTo("/StockOpeningEntry");
        }

        // Search logic just for Radzen Grid filtering if needed
    }
}
