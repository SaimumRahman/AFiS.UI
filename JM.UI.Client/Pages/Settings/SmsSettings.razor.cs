using JM.UI.Entities.Model.Sms;
using JM.UI.Service.UnitOfWork;
using JM.UIWeb.CustomBase;
using JM.Infrastructure.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace JM.UI.Client.Pages.Settings
{
    public partial class SmsSettingsComponent : PosComponentBase
    {
        [Inject] public IServiceUnitOfWork _serviceUnitOfWork { get; set; } = default!;

        protected SmsSettingsDTO Settings = new();
        protected bool IsLoading;
        protected bool IsSaving;
        protected bool ShowPassword;
        protected string TestMobile = string.Empty;
        protected string TestMessage = string.Empty;
        protected string TestResult = string.Empty;
        protected bool TestSuccess;

        protected int CurrentUserId;

        protected override async Task OnInitializedAsync()
        {
            await TokenService.InitializeTokenAsync();
            CurrentUserId = await GetLocalStorageInt("UserId");
            await LoadAsync();
        }

        protected async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                Settings = await _serviceUnitOfWork.SmsSettingsService.GetAsync();
                if (Settings.Templates == null || Settings.Templates.Count == 0)
                    Settings.Templates = new List<SmsTemplateDTO>();
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", $"Failed to load SMS settings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        protected async Task SaveAsync()
        {
            IsSaving = true;
            try
            {
                var res = await _serviceUnitOfWork.SmsSettingsService.SaveAsync(Settings, CurrentUserId);
                if (res != null && res.IsSuccessStatus)
                {
                    notificationService.Notify(NotificationSeverity.Success, "Saved", res.Message);
                    await LoadAsync();
                }
                else
                {
                    notificationService.Notify(NotificationSeverity.Error, "Error", res?.Message ?? "Save failed.");
                }
            }
            catch (Exception ex)
            {
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
            finally
            {
                IsSaving = false;
                StateHasChanged();
            }
        }

        protected async Task CancelAsync()
        {
            await LoadAsync();
        }

        protected async Task SendTestAsync()
        {
            TestResult = string.Empty;
            try
            {
                var res = await _serviceUnitOfWork.SmsSettingsService.TestSendAsync(Settings, TestMobile, TestMessage, CurrentUserId);
                TestSuccess = res != null && res.Success;
                TestResult = res?.ProviderMessage ?? (TestSuccess ? "Sent" : "Failed");
                notificationService.Notify(TestSuccess ? NotificationSeverity.Success : NotificationSeverity.Error,
                    TestSuccess ? "Test SMS sent" : "Test SMS failed", TestResult);
            }
            catch (Exception ex)
            {
                TestSuccess = false;
                TestResult = ex.Message;
                notificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
            }
        }

        protected void LoadDefault(SmsTemplateDTO t)
        {
            if (t != null && !string.IsNullOrWhiteSpace(t.DefaultText))
                t.TemplateText = t.DefaultText;
        }

        protected void InsertTag(SmsTemplateDTO t, string tag)
        {
            // Append at end as a safe fallback; caret insertion handled by JS interop when available.
            t.TemplateText = (t.TemplateText ?? string.Empty) + tag;
        }

        protected int CharCount(SmsTemplateDTO t) => (t?.TemplateText ?? string.Empty).Length;
        protected int Segments(SmsTemplateDTO t)
        {
            var len = CharCount(t);
            return len > 0 ? (int)Math.Ceiling(len / 160.0) : 0;
        }

        protected async Task<int> GetLocalStorageInt(string key)
        {
            try
            {
                var r = await _localStorage.GetAsync<string>(key);
                if (r.Success && int.TryParse(r.Value, out var v)) return v;
            }
            catch { }
            return 0;
        }

        public void Dispose() { }
    }
}
