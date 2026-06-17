using JM.UI.Entities.Model.Approval;
using JM.UI.Entities.ViewModel;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.Approval
{
    public partial class ApprovalLevelApproverAddComponent : AddEditPageBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        [Parameter] public int? Id { get; set; }

        protected ApprovalLevelApproverModelDTO ApprovalLevelApprover { get; set; } = new();
        protected bool IsProcessing { get; set; } = false;
        protected bool IsLoading { get; set; } = false;
        protected bool IsEditMode => Id.HasValue && Id.Value > 0;
        protected string PageTitle => IsEditMode ? "Edit Approval Level Approver" : "Add New Approval Level Approver";
        protected string PageIcon => IsEditMode ? "edit" : "person_add";
        protected List<ApprovalLevelModelDTO> ApprovalLevels { get; set; } = new();
        protected List<UserAuthDetailsDAO> Users { get; set; } = new();
        protected string SelectedLevelDetails { get; set; } = string.Empty;
        protected string SelectedUsers { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            await LoadApprovalLevels();
            await LoadUsers();

            if (IsEditMode)
            {
                await LoadApprovalLevelApprover();
            }
            else
            {
                InitializeApprovalLevelApprover();
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                
                Users = (await _serviceUnitOfWork.ApprovalLevelService.GetUser()).ToList(); 
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load users: {ex.Message}");
                Users = new List<UserAuthDetailsDAO>();
            }
        }
        private async Task LoadApprovalLevels()
        {
            try
            {
                ApprovalLevels = (await _serviceUnitOfWork.ApprovalLevelService.GetApprovalLevels()).ToList();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approval levels: {ex.Message}");
            }
        }

        private async Task LoadApprovalLevelApprover()
        {
            try
            {
                IsLoading = true;
                var approver = await _serviceUnitOfWork.ApprovalLevelApproverService.GetApprovalLevelApproverById(Id!.Value);

                if (approver == null)
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", "Approver not found.");
                    NavigationManager.NavigateTo("/ApprovalLevelApproverList");
                    return;
                }

                ApprovalLevelApprover = approver;
                UpdateSelectedLevelDetails();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load approver: {ex.Message}");
                NavigationManager.NavigateTo("/ApprovalLevelApproverList");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void InitializeApprovalLevelApprover()
        {
            ApprovalLevelApprover = _serviceUnitOfWork.ApprovalLevelApproverService.CreateNewApprovalLevelApprover();
        }

        protected void OnApprovalLevelChanged(object value)
        {
            UpdateSelectedLevelDetails();
            StateHasChanged();
        }
        protected void OnUserChanged(object value)
        {
            UpdateSelectedUser();
            StateHasChanged();
        }

        private void UpdateSelectedLevelDetails()
        {
            var selectedLevel = ApprovalLevels.FirstOrDefault(l => l.Id == ApprovalLevelApprover.ApprovalLevelID);
            if (selectedLevel != null)
            {
                SelectedLevelDetails = $"Workflow: {selectedLevel.WorkflowName} | Level: {selectedLevel.LevelNumber} - {selectedLevel.LevelName} | " +
                                       $"Required Approvers: {selectedLevel.RequiredApprovers} | " +
                                       $"Type: {(selectedLevel.IsParallelApproval ? "Parallel" : "Sequential")}";
            }
            else
            {
                SelectedLevelDetails = string.Empty;
            }
        }

        private void UpdateSelectedUser()
        {
            var selectedUser = Users.FirstOrDefault(u => u.UserId == ApprovalLevelApprover.UserID);
            SelectedUsers = selectedUser != null ? $"Selected: {selectedUser.UserName} ({selectedUser.Email})" : string.Empty;
        }

        protected async Task Save()
        {
            var userObj = await sessionStorage.GetAsync<string>("UserId");
            int userId = 0;

            if (!string.IsNullOrEmpty(userObj.Value))
            {
                int.TryParse(userObj.Value, out userId);
            }

            if (IsEditMode)
            {
                ApprovalLevelApprover.LastModifiedBy = userId;
            }
            else
            {
                ApprovalLevelApprover.CreatedBy = userId;
            }

            var validation = await _serviceUnitOfWork.ApprovalLevelApproverService.ValidateApprovalLevelApprover(ApprovalLevelApprover);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.ApprovalLevelApproverService.SaveUpdateApprovalLevelApprover(ApprovalLevelApprover);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success",
                        IsEditMode ? "Approver updated successfully!" : "Approver created successfully!");
                    NavigationManager.NavigateTo("/ApprovalLevelApproverList");
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approver: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected async Task SaveAndNew()
        {
            if (IsEditMode)
            {
                await Save();
                return;
            }

            var userObj = await sessionStorage.GetAsync<string>("UserId");
            int userId = 0;

            if (!string.IsNullOrEmpty(userObj.Value))
            {
                int.TryParse(userObj.Value, out userId);
            }

            ApprovalLevelApprover.CreatedBy = userId;

            var validation = await _serviceUnitOfWork.ApprovalLevelApproverService.ValidateApprovalLevelApprover(ApprovalLevelApprover);
            if (!validation.IsValid)
            {
                notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await _serviceUnitOfWork.ApprovalLevelApproverService.SaveUpdateApprovalLevelApprover(ApprovalLevelApprover);

                if (result.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Success", "Approver created successfully!");
                    InitializeApprovalLevelApprover();
                    StateHasChanged();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save approver: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        protected void Cancel()
        {
            NavigationManager.NavigateTo("/ApprovalLevelApproverList");
        }

        protected async Task Reset()
        {
            if (IsEditMode)
            {
                await LoadApprovalLevelApprover();
            }
            else
            {
                InitializeApprovalLevelApprover();
            }
            StateHasChanged();
        }
    }
}