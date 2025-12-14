using JM.UI.Entities.Model.Alert;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UIWeb.Pages.Dialog
{
    public partial class AlertDialogComponent : ComponentBase
    {
        [Inject] public DialogService DialogService { get; set; } = default!;

        [Parameter]
        public string Messages { get; set; } = string.Empty;

        public AlertDAO alerts { get; set; } = new();

        protected override void OnInitialized()
        {
            alerts = new AlertDAO();
            alerts.Messages = Messages;
        }

        public void CloseDialog()
        {
            DialogService.Close();
        }
    }
}