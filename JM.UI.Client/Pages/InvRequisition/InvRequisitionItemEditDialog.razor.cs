using JM.UI.Entities.Model.InvRequisition;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace JM.UI.Client.Pages.InvRequisition
{
    public partial class InvRequisitionItemEditDialogComponent : ComponentBase
    {
        [Parameter] public InvRequisitionDetailDTO Detail { get; set; } = new();
        [Inject] public DialogService DialogService { get; set; } = default!;
    }
}
