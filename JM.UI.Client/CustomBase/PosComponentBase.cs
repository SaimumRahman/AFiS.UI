using JM.UI.Client.Services;
using JM.UI.Entities.Services;
using JM.UI.Entities.ViewModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using Radzen;

namespace JM.UIWeb.CustomBase
{
    public class PosComponentBase : ComponentBase
    {
        [Inject] public ITokenProvider TokenProvider { get; set; }
        [Inject] public ITokenService TokenService { get; set; } = default!; // ✅ Add this
        [Inject]
        protected HttpClient httpClient { get; set; }
        [Inject] public TooltipService TooltipService { get; set; } = default!;
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject]
        protected ProtectedSessionStorage sessionStorage { get; set; }
        [Inject]
        protected ProtectedLocalStorage _localStorage { get; set; }

        [Inject]
        protected NotificationService notificationService { get; set; }


        [Inject]
        protected DialogService dialogService { get; set; }


        [Inject]
        IConfiguration configuration { get; set; }

        [Inject]
        protected IJSRuntime jsRuntimes { get; set; }

        public LoggedInfo loggedInfos { get; set; }
    }
}