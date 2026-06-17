using JM.UI.Entities.Model.MembershipType;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.MembershipType
{
    public partial class MembershipTypeListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected RadzenDataGrid<MembershipTypeDTO> MembershipTypesGrid = default!;
        protected IEnumerable<MembershipTypeDTO> MembershipTypes { get; set; } = new List<MembershipTypeDTO>();
        protected bool IsLoading { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadMembershipTypes();
        }

        private async Task LoadMembershipTypes()
        {
            try
            {
                IsLoading = true;
                MembershipTypes = await _serviceUnitOfWork.MembershipTypeService.GetAll();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load membership types: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected void AddMembershipType()
        {
            NavigationManager.NavigateTo("/MembershipTypeAdd");
        }

        protected void EditMembershipType(MembershipTypeDTO membershipType)
        {
            NavigationManager.NavigateTo($"/MembershipTypeAdd/{membershipType.Id}");
        }

        protected async Task DeleteMembershipType(MembershipTypeDTO membershipType)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete membership type '{membershipType.Name}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.MembershipTypeService.Delete(membershipType.Id);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Membership type deleted successfully.");
                    await LoadMembershipTypes();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete membership type.");
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
            MembershipTypesGrid?.Dispose();
        }
    }
}
