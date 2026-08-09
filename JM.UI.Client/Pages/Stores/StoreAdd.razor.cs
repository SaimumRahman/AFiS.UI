using JM.UI.Entities.Model.FinancialAccounts;
using JM.UI.Entities.Model.Stores;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;

namespace JM.UI.Client.Pages.Store;

public partial class StoreAddComponent : AddEditPageBase
{
    [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;

    [Parameter] public int? Id { get; set; }

    protected StoreDTO Store { get; set; } = new();
    protected List<FinancialAccountDropdownDTO> FinancialAccounts { get; set; } = new();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsEditMode => Id.HasValue && Id.Value > 0;
    protected string PageTitle => IsEditMode ? "Edit Store" : "Add New Store";
    protected string PageIcon => IsEditMode ? "edit" : "store";

    protected string LetterHeadFileName { get; set; } = string.Empty;
    protected bool IsImageFile { get; set; } = false;

    private static readonly long MaxFileSize = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".bmp", ".gif", ".webp" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };

    protected override async Task OnInitializedAsync()
    {
        await TokenService.InitializeTokenAsync();
        await LoadFinancialAccounts();

        if (IsEditMode)
        {
            await LoadStore();
        }
        else
        {
            InitializeStore();
        }
    }

    private async Task LoadFinancialAccounts()
    {
        try
        {
            FinancialAccounts = (await _serviceUnitOfWork.FinancialAccountsService.GetFinancialAccountsForDropdown()).ToList();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load financial accounts: {ex.Message}");
        }
    }

    private async Task LoadStore()
    {
        try
        {
            IsLoading = true;
            var store = await _serviceUnitOfWork.StoreService.GetStoreById(Id!.Value);

            if (store == null)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", "Store not found.");
                NavigationManager.NavigateTo("/StoreList");
                return;
            }

            Store = store;

            // Restore file state if base64 exists
            if (!string.IsNullOrEmpty(Store.LetterHeadFile))
            {
                DetectFileTypeFromBase64(Store.LetterHeadFile);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load store: {ex.Message}");
            NavigationManager.NavigateTo("/StoreList");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializeStore()
    {
        Store = _serviceUnitOfWork.StoreService.CreateNewStore();
        LetterHeadFileName = string.Empty;
        IsImageFile = false;
    }

    protected async Task TriggerFileInput()
    {
        await JS.InvokeVoidAsync("eval", "document.getElementById('letterheadInput').click()");
    }

    protected async Task OnLetterHeadFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;

        var ext = Path.GetExtension(file.Name).ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
        {
            notificationService.Notify(NotificationSeverity.Warning, "Invalid File", "Allowed: jpg, jpeg, png, pdf, bmp, gif, webp.");
            return;
        }

        if (file.Size > MaxFileSize)
        {
            notificationService.Notify(NotificationSeverity.Warning, "File Too Large", "Maximum file size is 2 MB.");
            return;
        }

        try
        {
            using var stream = file.OpenReadStream(MaxFileSize);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var mimeType = GetMimeType(ext);
            var base64 = Convert.ToBase64String(bytes);

            Store.LetterHeadFile = $"data:{mimeType};base64,{base64}";
            LetterHeadFileName = file.Name;
            IsImageFile = ImageExtensions.Contains(ext);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to read file: {ex.Message}");
        }
    }

    protected void ClearLetterHeadFile()
    {
        Store.LetterHeadFile = null;
        LetterHeadFileName = string.Empty;
        IsImageFile = false;
        StateHasChanged();
    }

    private void DetectFileTypeFromBase64(string dataUrl)
    {
        // e.g. "data:image/png;base64,..."
        if (dataUrl.StartsWith("data:image/"))
        {
            IsImageFile = true;
            var start = dataUrl.IndexOf('/') + 1;
            var end = dataUrl.IndexOf(';');
            var ext = dataUrl.Substring(start, end - start); // "png", "jpeg", etc.
            LetterHeadFileName = $"letterhead.{ext}";
        }
        else if (dataUrl.StartsWith("data:application/pdf"))
        {
            IsImageFile = false;
            LetterHeadFileName = "letterhead.pdf";
        }
        else
        {
            IsImageFile = false;
            LetterHeadFileName = "letterhead file";
        }
    }

    private static string GetMimeType(string ext) => ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".bmp" => "image/bmp",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };

    // ── Save / SaveAndNew / Cancel / Reset unchanged below ──

    protected async Task Save()
    {
        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int userId = 0;
        if (!string.IsNullOrEmpty(userObj.Value)) int.TryParse(userObj.Value, out userId);

        if (IsEditMode) Store.ModifiedBy = userId.ToString();
        else Store.CreatedBy = userId.ToString();

        var validation = await _serviceUnitOfWork.StoreService.ValidateStore(Store);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.StoreService.SaveUpdateStore(Store);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success",
                    IsEditMode ? "Store updated successfully!" : "Store created successfully!");
                NavigationManager.NavigateTo("/StoreList");
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save store: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected async Task SaveAndNew()
    {
        if (IsEditMode) { await Save(); return; }

        var userObj = await sessionStorage.GetAsync<string>("UserId");
        int userId = 0;
        if (!string.IsNullOrEmpty(userObj.Value)) int.TryParse(userObj.Value, out userId);

        Store.CreatedBy = userId.ToString();

        var validation = await _serviceUnitOfWork.StoreService.ValidateStore(Store);
        if (!validation.IsValid)
        {
            notificationService.Notify(NotificationSeverity.Warning, "Validation", validation.ErrorMessage);
            return;
        }

        try
        {
            IsProcessing = true;
            var result = await _serviceUnitOfWork.StoreService.SaveUpdateStore(Store);

            if (result.IsSuccessStatus)
            {
                notificationService.Notify(NotificationSeverity.Success, "Success", "Store created successfully!");
                InitializeStore();
                StateHasChanged();
            }
            else
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", result.Message);
            }
        }
        catch (Exception ex)
        {
            notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to save store: {ex.Message}");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    protected void Cancel() => NavigationManager.NavigateTo("/StoreList");

    protected async Task Reset()
    {
        if (IsEditMode) await LoadStore();
        else InitializeStore();
        StateHasChanged();
    }
}