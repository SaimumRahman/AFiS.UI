using JM.UI.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace JM.UI.Client.Pages.Users
{
    public partial class LogoutUserComponent : ComponentBase
    {
        [Inject]
        ITokenService tokenService { get; set; }

        [Inject]
        ProtectedSessionStorage sessionStorage { get; set; }

        [Inject]
        NavigationManager navigationManager { get; set; }

        [Inject]
        IJSRuntime jsRuntimes { get; set; }

        [Inject]
        AuthenticationStateProvider AuthStateProvider { get; set; }

        protected bool isLoggingOut = false;

        protected override async Task OnInitializedAsync()
        {
            await PerformLogout();
        }

        private async Task PerformLogout()
        {
            if (isLoggingOut)
                return;

            try
            {
                isLoggingOut = true;
                StateHasChanged();

                Console.WriteLine("═══════════════════════════════════");
                Console.WriteLine("🔓 Attempting logout...");

                // Clear token using TokenService
                await tokenService.ClearTokenAsync();

                Console.WriteLine("✅ Token cleared successfully");

                // Clear all session storage
                await sessionStorage.DeleteAsync("UserId");
                await sessionStorage.DeleteAsync("UserInfo");
                await sessionStorage.DeleteAsync("CompanyId");

                Console.WriteLine("✅ All session data cleared");

                // Notify authentication state changed
                if (AuthStateProvider is CustomAuthenticationStateProvider customAuthProvider)
                {
                    customAuthProvider.NotifyUserLogout();
                    Console.WriteLine("✅ Authentication state updated");
                }

                Console.WriteLine("✅ Logout successful!");
                Console.WriteLine("🚀 Redirecting to login page...");
                Console.WriteLine("═══════════════════════════════════");

                // Navigate to login page
                navigationManager.NavigateTo("/LoginUser", true);
            }
            catch (Exception e)
            {
                Console.WriteLine("═══════════════════════════════════");
                Console.WriteLine($"❌ Logout error: {e.Message}");
                Console.WriteLine($"Stack trace: {e.StackTrace}");
                Console.WriteLine("═══════════════════════════════════");

                // Even if error, redirect to login
                navigationManager.NavigateTo("/LoginUser", true);
            }
            finally
            {
                isLoggingOut = false;
            }
        }
    }
}