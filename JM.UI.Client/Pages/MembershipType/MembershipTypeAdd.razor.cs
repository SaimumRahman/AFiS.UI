using JM.UI.Entities.Model.MembershipType;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.MembershipType;

public partial class MembershipTypeAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected MembershipTypeDTO MembershipType { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Membership Type" : "Add New Membership Type";
    protected string PageIcon => IsEditMode ? "edit" : "card_membership";

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();

        if (IsEditMode)
        {
            await LoadMembershipType();
        }
        else
        {
            InitializeMembershipType();
        }
    }

    private async Task LoadMembershipType()
    {
        try
        {
            IsLoading = true;
            var membershipType = await _serviceUnitOfWork.MembershipTypeService.GetById(Id!.Value);

            if (membershipType == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Membership type not found.");
                NavigationManager.NavigateTo("/MembershipTypeList");
                return;
            }

            MembershipType = membershipType;
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load membership type: {ex.Message}");
            NavigationManager.NavigateTo("/MembershipTypeList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeMembershipType()
    {
        MembershipType = _serviceUnitOfWork.MembershipTypeService.CreateNew();
    }

    protected async Task Save()
    {
        var validation = await _serviceUnitOfWork.MembershipTypeService.Validate(MembershipType);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.MembershipTypeService.SaveUpdate(MembershipType);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Membership type updated successfully!" : "Membership type created successfully!");
                NavigationManager.NavigateTo("/MembershipTypeList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save membership type: {ex.Message}");
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

        var validation = await _serviceUnitOfWork.MembershipTypeService.Validate(MembershipType);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.MembershipTypeService.SaveUpdate(MembershipType);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Membership type created successfully!");
                InitializeMembershipType();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save membership type: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/MembershipTypeList");
    }

    protected async Task Reset()
    {
        if (IsEditMode)
        {
            await LoadMembershipType();
        }
        else
        {
            InitializeMembershipType();
        }
        StateHasChanged();
    }
}
