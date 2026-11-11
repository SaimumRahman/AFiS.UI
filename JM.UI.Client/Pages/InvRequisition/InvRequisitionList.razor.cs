using JM.UI.Entities.Model.InvRequisition;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.InvRequisition
{
    public partial class InvRequisitionListComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
        [Inject] public ProtectedLocalStorage _localStorage { get; set; } = default!;

        protected RadzenDataGrid<InvRequisitionMasterDTO> RequisitionsGrid = default!;
        protected IEnumerable<InvRequisitionMasterDTO> Requisitions { get; set; } = new List<InvRequisitionMasterDTO>();
        protected IEnumerable<RequisitionStatusDTO> Statuses { get; set; } = new List<RequisitionStatusDTO>();
        protected bool IsLoading { get; set; } = false;
        protected HashSet<int> LoadingDetails { get; set; } = new();
        protected int CurrentUserId { get; set; }
        protected int UserStoreId { get; set; }
        protected bool IsSystemGroupUser { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadUserContext();
            await LoadRequisitions();
        }

        private async Task LoadUserContext()
        {
            try
            {
                var userIdResult = await _localStorage.GetAsync<string>("UserId");
                var storeIdResult = await _localStorage.GetAsync<string>("StoreId");
                CurrentUserId = int.TryParse(userIdResult.Value, out var uid) ? uid : 0;
                UserStoreId = int.TryParse(storeIdResult.Value, out var sid) ? sid : 0;
			try
			{
				IsSystemGroupUser = await _serviceUnitOfWork.UserGroupService
					.GetSystemGroupCountByUserId(CurrentUserId) > 0;
			}
			catch { IsSystemGroupUser = false; }
            }
            catch { }
        }

        private async Task LoadRequisitions()
        {
            try
            {
                IsLoading = true;

                if (IsSystemGroupUser || CurrentUserId == 1)
                    Requisitions = await _serviceUnitOfWork.InvRequisitionService.GetAll();
                else
                    Requisitions = await _serviceUnitOfWork.InvRequisitionService.GetAllByStoreId(UserStoreId);

                Statuses = await _serviceUnitOfWork.InvRequisitionService.GetRequisitionStatuses();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load requisitions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task OnRowExpand(InvRequisitionMasterDTO requisition)
        {
            if (requisition.Details?.Count > 0) return;

            try
            {
                LoadingDetails.Add(requisition.RequisitionID);
                StateHasChanged();

                var full = await _serviceUnitOfWork.InvRequisitionService.GetById(requisition.RequisitionID);
                if (full != null)
                {
                    requisition.Details = full.Details;
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load details: {ex.Message}");
            }
            finally
            {
                LoadingDetails.Remove(requisition.RequisitionID);
                StateHasChanged();
            }
        }

        protected void AddRequisition()
        {
            NavigationManager.NavigateTo("/InvRequisitionAdd");
        }

        protected async Task OnStatusChanged(object args, InvRequisitionMasterDTO item)
        {
            var newStatusId = args as int?;
            if (!newStatusId.HasValue || newStatusId.Value == item.StatusID) return;

            var oldStatusId = item.StatusID;
            var selectedStatus = Statuses.FirstOrDefault(s => s.Id == newStatusId.Value);
            string? rejectNotes = null;

            if (selectedStatus?.StatusName?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true)
            {
                rejectNotes = await dialogService.OpenAsync<RejectNotesDialog>("Rejection Notes");
                if (string.IsNullOrWhiteSpace(rejectNotes))
                {
                    item.StatusID = oldStatusId;
                    StateHasChanged();
                    return;
                }
            }

            await _serviceUnitOfWork.InvRequisitionService.UpdateRequisitionStatus(item.RequisitionID, selectedStatus.Id, CurrentUserId, rejectNotes);
            notificationService.Notify(NotificationSeverity.Info, "Status Change",
                $"Requisition {item.RequisitionNo} changed to {selectedStatus?.StatusName}" +
                (rejectNotes != null ? $" â€” Notes: {rejectNotes}" : ""));
        }

        protected void EditRequisition(InvRequisitionMasterDTO requisition)
        {
            NavigationManager.NavigateTo($"/InvRequisitionAdd/{requisition.RequisitionID}");
        }

        protected void CreateTransfer(InvRequisitionMasterDTO requisition)
        {
            NavigationManager.NavigateTo($"/ItemTransferEntry/FromRequisition/{requisition.RequisitionID}");
        }

        protected async Task DeleteRequisition(InvRequisitionMasterDTO requisition)
        {
            var confirm = await dialogService.Confirm(
                $"Are you sure you want to delete requisition '{requisition.RequisitionNo}'?",
                "Confirm Delete",
                new ConfirmOptions { OkButtonText = "Yes, Delete", CancelButtonText = "Cancel" });

            if (confirm == true)
            {
                var result = await _serviceUnitOfWork.InvRequisitionService.Delete(requisition.RequisitionID);
                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", result.Message ?? "Requisition deleted successfully.");
                    await LoadRequisitions();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message ?? "Failed to delete requisition.");
                }
            }
        }

        protected string Truncate(string? value, int maxChars)
            => string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";

        // Direction is relative to the signed-in user's branch:
        //  - "Requested by us"  -> our branch is the ToStore (we raised the request)
        //  - "Requested from us"-> our branch is the FromStore (we must issue the goods)
        protected string DirectionLabel(InvRequisitionMasterDTO requisition)
        {
            if (UserStoreId <= 0) return "-";

            var isTo = requisition.ToStore == UserStoreId;
            var isFrom = requisition.FromStore == UserStoreId;

            if (isTo && isFrom) return "Internal";
            if (isTo) return "Requested by us";
            if (isFrom) return "Requested from us";
            return "-";
        }

        protected BadgeStyle DirectionStyle(InvRequisitionMasterDTO requisition)
        {
            if (UserStoreId <= 0) return BadgeStyle.Light;

            var isTo = requisition.ToStore == UserStoreId;
            var isFrom = requisition.FromStore == UserStoreId;

            if (isTo && isFrom) return BadgeStyle.Secondary;
            if (isTo) return BadgeStyle.Primary;
            if (isFrom) return BadgeStyle.Warning;
            return BadgeStyle.Light;
        }

        protected void ShowTooltip(ElementReference elementReference, string text)
        {
            TooltipService.Open(elementReference, text, new TooltipOptions { Position = TooltipPosition.Top });
        }


        protected async Task RejectRequisition(InvRequisitionMasterDTO requisition)
        {
            if (requisition == null) return;

            var reason = await dialogService.OpenAsync<RejectNotesDialog>("Rejection Notes");
            if (string.IsNullOrWhiteSpace(reason))
            {
                notificationService.Notify(NotificationSeverity.Warning, "Cancelled", "Rejection cancelled — a reason is required.", 3000);
                return;
            }

            int refundMethodId = 0;
            string? refundTxnNo = null;
            decimal advance = requisition.ToStore.HasValue ? 0 : 0; // placeholder; server computes

            var result = await _serviceUnitOfWork.InvRequisitionService.Reject(
                requisition.RequisitionID, reason!, CurrentUserId, UserStoreId, IsSystemGroupUser,
                refundMethodId, refundTxnNo);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Rejected",
                    result.Message ?? "Requisition rejected.", 4000);
                await LoadRequisitions();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error",
                    result.Message ?? "Failed to reject requisition.", 4000);
            }
        }
        public void Dispose()
        {
            RequisitionsGrid?.Dispose();
        }
    }
}

