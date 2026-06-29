using JM.UI.Entities.Model.Coupon;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Coupon
{
    public partial class CouponTypeListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CouponTypeDTO> CouponTypesGrid = default!;
        protected IEnumerable<CouponTypeDTO> CouponTypes { get; set; } = new List<CouponTypeDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCouponTypes();
        }

        private async Task LoadCouponTypes()
        {
            try
            {
                IsLoading = true;
                CouponTypes = await _serviceUnitOfWork.CouponTypeService.GetAll();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load coupon types: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddCouponType() => NavigationManager.NavigateTo("/CouponTypeAdd");

        protected void EditCouponType(CouponTypeDTO couponType)
            => NavigationManager.NavigateTo($"/CouponTypeAdd/{couponType.Id}");

        protected async Task DeleteCouponType(CouponTypeDTO couponType)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete coupon type '{couponType.TypeName}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.CouponTypeService.Delete(couponType.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Coupon type deleted successfully.");
                    await LoadCouponTypes();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete coupon type.");
                }
            }
        }

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }

        public void Dispose()
        {
            CouponTypesGrid?.Dispose();
        }
    }
}
