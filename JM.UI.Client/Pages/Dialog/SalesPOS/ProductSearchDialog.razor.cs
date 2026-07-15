using JM.UI.Entities.Model.SalesPOS;
using JM.UI.Service.UnitOfWork;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace JM.UI.Client.Pages.Dialog.SalesPOS
{
    public partial class ProductSearchDialogComponent : ComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public DialogService DialogService { get; set; } = default!;

        [Parameter] public string InitialSearchTerm { get; set; } = "";

        protected string GlobalSearchTerm { get; set; } = "";
        protected List<ProductSearchDTO> GlobalSearchResults { get; set; } = new();
        protected Radzen.Blazor.RadzenDataGrid<ProductSearchDTO> ProductSearchGrid = default!;

        protected override void OnInitialized()
        {
            GlobalSearchTerm = InitialSearchTerm;
        }

        protected async Task OnGlobalSearch()
        {
            if (string.IsNullOrWhiteSpace(GlobalSearchTerm)) return;
            GlobalSearchResults = (await _serviceUnitOfWork.SaleService.SearchProducts(GlobalSearchTerm)).ToList();
        }

        protected async Task OnGlobalSearchKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await OnGlobalSearch();
        }

        protected void SelectSearchedProduct(ProductSearchDTO product)
        {
            DialogService.Close(product);
        }

        protected void Cancel()
        {
            DialogService.Close(null);
        }
    }
}
