using JM.UI.Entities.Model.Coupon;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Coupon
{
    public partial class CouponListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<CouponDTO> CouponsGrid = default!;
        protected IEnumerable<CouponDTO> Coupons { get; set; } = new List<CouponDTO>();
        protected bool IsLoading { get; set; } = false;
        protected HashSet<int> LoadingDetails { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadCoupons();
        }

        private async Task LoadCoupons()
        {
            try
            {
                IsLoading = true;
                Coupons = await _serviceUnitOfWork.CouponService.GetAll();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load coupons: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddCoupon() => NavigationManager.NavigateTo("/CouponAdd");

        protected void EditCoupon(CouponDTO coupon)
            => NavigationManager.NavigateTo($"/CouponAdd/{coupon.Id}");

        protected async Task DeleteCoupon(CouponDTO coupon)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete coupon '{coupon.CouponCode}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.CouponService.Delete(coupon.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Coupon deleted successfully.");
                    await LoadCoupons();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete coupon.");
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
            CouponsGrid?.Dispose();
        }
    }
}
