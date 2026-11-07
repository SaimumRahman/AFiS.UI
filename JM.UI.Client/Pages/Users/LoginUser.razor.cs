using JM.UI.Client.Services;
using JM.UI.Entities.Model.Users;
using JM.UI.Service.UnitOfWork;
using JM.UI.Service.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Radzen;

namespace JM.UI.Client.Pages.Users
{
    public partial class LoginUserComponent : ComponentBase
    {
        [Inject] IUserAuthService userAuthService { get; set; }
        [Inject] IServiceUnitOfWork serviceUnitOfWork { get; set; }
        [Inject] ITokenService tokenService { get; set; }
        [Inject] ProtectedSessionStorage sessionStorage { get; set; }

        [Inject] ProtectedLocalStorage _localStorage { get; set; }
        [Inject] NavigationManager navigationManager { get; set; }
        [Inject] IJSRuntime jsRuntimes { get; set; }
        [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] NotificationService notificationService { get; set; }

        public LoginRequest loginRequestDAO { get; set; }
        private AuthenticatedUserResponse response { get; set; }
        protected bool isLoading = false;

        protected string errorMessage { get; set; }
        protected bool showError { get; set; } = false;

        protected async Task OnClick(string buttonName)
        {
            if (buttonName == "Success button")
            {
                if (isLoading)
                    return;

                var navigated = false;

                try
                {
                    isLoading = true;
                    showError = false;
                    errorMessage = string.Empty;
                    StateHasChanged();

                    Console.WriteLine("🔐 Attempting login...");

                    response = await userAuthService.Login(loginRequestDAO);

                    // Check the login result BEFORE calling any other API. On a failed
                    // login there is no token, so an authenticated call (GetEmployeeBySurname)
                    // would throw 401 and replace the real error with a generic one.
                    if (response == null || string.IsNullOrEmpty(response.Token))
                    {
                        Console.WriteLine("❌ Login failed - no token received");
                        SetError(response?.ErrorMessage);
                        return;
                    }

                    var dataUser = await serviceUnitOfWork.EmployeeService
                                       .GetEmployeeBySurname(loginRequestDAO.LoginId);

                    Console.WriteLine("═══════════════════════════════════");
                    Console.WriteLine($"✅ Login successful!");
                    Console.WriteLine($"Token received: {response.Token.Substring(0, Math.Min(20, response.Token.Length))}...");
                    Console.WriteLine($"User: {response.Username}");
                    Console.WriteLine($"Company ID: {response.CompanyId}");
                    Console.WriteLine("═══════════════════════════════════");

                    // Save token using TokenService
                    await tokenService.SetTokenAsync(response.Token);
                    await tokenService.SetRefreshTokenAsync(response.RefreshToken);
                    await sessionStorage.SetAsync("UserId", response.UserId.ToString());
                    await sessionStorage.SetAsync("UserInfo", JsonConvert.SerializeObject(response));
                    await _localStorage.SetAsync("UserId", response.UserId.ToString());
                    await _localStorage.SetAsync("UserInfo", JsonConvert.SerializeObject(response));
                    await _localStorage.SetAsync("RefreshToken", response.RefreshToken);

                    // ✅ FIXED: Save StoreId safely with null/zero check
                    if (dataUser == null)
                    {
                        Console.WriteLine($"❌ No employee found for LoginId: {loginRequestDAO.LoginId}");
                        await _localStorage.SetAsync("StoreId", "0");
                    }
                    else if (dataUser.StoreId == null || dataUser.StoreId == 0)
                    {
                        Console.WriteLine($"❌ Employee {dataUser.Id} has no StoreId assigned in DB");
                        await _localStorage.SetAsync("StoreId", "0");
                    }
                    else
                    {
                        Console.WriteLine($"✅ StoreId = {dataUser.StoreId} saved for UserId = {response.UserId}");
                        await _localStorage.SetAsync("StoreId", dataUser.StoreId.ToString());
                    }

                    if (response.CompanyId > 0)
                    {
                        await sessionStorage.SetAsync("CompanyId", response.CompanyId.ToString());
                    }

                    var permissions = await serviceUnitOfWork.GroupRoutePermissionService
                                          .GetRouteListByUserId(response.UserId);

                    await _localStorage.SetAsync(
                        "Permissions",
                        JsonConvert.SerializeObject(permissions)
                    );

                    Console.WriteLine("✅ All credentials saved successfully");

                    ((CustomAuthenticationStateProvider)AuthStateProvider).NotifyUserAuthentication();

                    Console.WriteLine("🚀 Navigating to dashboard...");

                    navigationManager.NavigateTo("/Dashboard", true);
                    navigated = true;
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine("═══════════════════════════════════");
                    Console.WriteLine($"❌ Network error: {httpEx.Message}");
                    Console.WriteLine("═══════════════════════════════════");

                    SetError("Unable to connect to the server. Please check your internet connection and try again.");
                }
                catch (UnauthorizedAccessException)
                {
                    SetError("Invalid username or password. Please try again.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("═══════════════════════════════════");
                    Console.WriteLine($"❌ Login error: {e.Message}");
                    Console.WriteLine($"Stack trace: {e.StackTrace}");
                    Console.WriteLine("═══════════════════════════════════");

                    SetError("An unexpected error occurred. Please try again later.");
                }
                finally
                {
                    isLoading = false;

                    // After a successful login the page is being replaced, so re-rendering
                    // here would only fight the navigation.
                    if (!navigated)
                        StateHasChanged();
                }
            }
        }

        /// <summary>
        /// Shows a message in the login error banner. A blank message would render an
        /// empty red strip, so fall back to a readable default.
        /// </summary>
        private void SetError(string errorMsg)
        {
            errorMessage = string.IsNullOrWhiteSpace(errorMsg)
                ? "Invalid username or password. Please try again."
                : errorMsg.Trim();

            showError = true;
            isLoading = false;
            StateHasChanged();
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