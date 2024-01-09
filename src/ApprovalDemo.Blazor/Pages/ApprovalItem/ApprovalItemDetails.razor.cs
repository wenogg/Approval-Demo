using System.Threading.Tasks;
using ApprovalDemo.ApprovalItems;
using Microsoft.AspNetCore.Components;

namespace ApprovalDemo.Blazor.Pages.ApprovalItem;

public partial class ApprovalItemDetails
{
    [Parameter]
    public string ApprovalItemId { get; set; } = default!;

    [Inject]
    private IApprovalItemAppService ApprovalItemAppService { get; set; } = default!;

    private ApprovalItemDto ApprovalItem { get; set; } = new();

    private bool Loading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Loading = true;
        var itemId = int.Parse(ApprovalItemId);
        ApprovalItem = await ApprovalItemAppService.GetAsync(itemId);
        Loading = false;
    }

    private async Task SendUserAction(string action)
    {
        Loading = true;
        var itemId = int.Parse(ApprovalItemId );
        await ApprovalItemAppService.ApplyTransition(itemId, action);
        ApprovalItem = await ApprovalItemAppService.GetAsync(itemId);
        Loading = false;
    }
}