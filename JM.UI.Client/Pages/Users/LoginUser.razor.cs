using JM.UI.Client.Services;
using JM.UI.Entities.Model.Users;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace JM.UI.Client.Pages.Users
{
    public partial class LoginUserComponent : ComponentBase
    {
        [Inject] IUserAuthService userAuthService { get; set; }
        [Inject] IServiceUnitOfWork  serviceUnitOfWork { get; set; }
        [Inject] ITokenService tokenService { get; set; }
        [Inject] ProtectedSessionStorage sessionStorage { get; set; }

        [Inject] ProtectedLocalStorage _localStorage { get; set; }
        [Inject] NavigationManager navigationManager { get; set; }
        [Inject] IJSRuntime jsRuntimes { get; set; }
        [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }

        public LoginRequest loginRequestDAO { get; set; }
        private AuthenticatedUserResponse response { get; set; }
        protected bool isLoading = false;

        // ✅ Add error message properties
        protected string errorMessage { get; set; }
        protected bool showError { get; set; } = false;

        protected async Task OnClick(string buttonName)
        {
            if (buttonName == "Success button")
            {
                if (isLoading)
                    return;

                try
                {
                    isLoading = true;
                    showError = false; // ✅ Reset error state
                    errorMessage = string.Empty;
                    StateHasChanged();

                    Console.WriteLine("🔐 Attempting login...");

                    response = await userAuthService.Login(loginRequestDAO);
                    var dataUser = await serviceUnitOfWork.EmployeeService.GetEmployeeBySurname(loginRequestDAO.LoginId);
                    if (response == null || string.IsNullOrEmpty(response.Token))
                    {
                        Console.WriteLine("❌ Login failed - no token received");

                        // ✅ Show error message
                        errorMessage = "Invalid username or password. Please try again.";
                        showError = true;

                        await jsRuntimes.InvokeVoidAsync("credentialAlert");
                        isLoading = false;
                        StateHasChanged();
                    }
                    else
                    {
                        Console.WriteLine("═══════════════════════════════════");
                        Console.WriteLine($"✅ Login successful!");
                        Console.WriteLine($"Token received: {response.Token.Substring(0, Math.Min(20, response.Token.Length))}...");
                        Console.WriteLine($"User: {response.Username}");
                        Console.WriteLine($"Company ID: {response.CompanyId}");
                        Console.WriteLine("═══════════════════════════════════");

                        // Save token using TokenService
                        await tokenService.SetTokenAsync(response.Token);
                        await sessionStorage.SetAsync("UserId", response.UserId.ToString());
                        await sessionStorage.SetAsync("UserInfo", JsonConvert.SerializeObject(response));
                        await _localStorage.SetAsync("UserId", response.UserId.ToString());
                        await _localStorage.SetAsync("StoreId", dataUser?.StoreId.ToString() ?? "0");
                        await _localStorage.SetAsync("UserInfo", JsonConvert.SerializeObject(response));

                        if (response.CompanyId > 0)
                        {
                            await sessionStorage.SetAsync("CompanyId", response.CompanyId.ToString());
                        }
                        var permissions = await serviceUnitOfWork.GroupRoutePermissionService.GetRouteListByUserId(response.UserId);

                        await _localStorage.SetAsync(
                            "Permissions",
                            JsonConvert.SerializeObject(permissions)
                        );

                        Console.WriteLine("✅ All credentials saved successfully");

                        // 🔐 Notify authentication state changed
                        ((CustomAuthenticationStateProvider)AuthStateProvider).NotifyUserAuthentication();

                        Console.WriteLine("🚀 Navigating to dashboard...");

                        // Navigate to dashboard
                        navigationManager.NavigateTo("/Dashboard", true);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    // ✅ Handle network/HTTP errors
                    Console.WriteLine("═══════════════════════════════════");
                    Console.WriteLine($"❌ Network error: {httpEx.Message}");
                    Console.WriteLine("═══════════════════════════════════");

                    errorMessage = "Unable to connect to the server. Please check your internet connection and try again.";
                    showError = true;
                    isLoading = false;
                    StateHasChanged();
                }
                catch (UnauthorizedAccessException)
                {
                    // ✅ Handle unauthorized access
                    errorMessage = "Invalid username or password. Please try again.";
                    showError = true;
                    isLoading = false;
                    StateHasChanged();
                }
                catch (Exception e)
                {
                    // ✅ Handle general errors
                    Console.WriteLine("═══════════════════════════════════");
                    Console.WriteLine($"❌ Login error: {e.Message}");
                    Console.WriteLine($"Stack trace: {e.StackTrace}");
                    Console.WriteLine("═══════════════════════════════════");

                    errorMessage = "An unexpected error occurred. Please try again later.";
                    showError = true;
                    isLoading = false;
                    StateHasChanged();
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
                    if (authState.User.Identity?.IsAuthenticated == true)
                    {
                        Console.WriteLine("✅ User already logged in, redirecting to dashboard");
                        navigationManager.NavigateTo("/dashboard", true);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Session check error: {ex.Message}");
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override void OnInitialized()
        {
            initializeObjects();
        }

        private void initializeObjects()
        {
            loginRequestDAO = new LoginRequest();
            response = new AuthenticatedUserResponse();
            errorMessage = string.Empty;
            showError = false;
        }
    }
}