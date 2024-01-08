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

    protected override async Task OnInitializedAsync()
    {
        var itemId = int.Parse(ApprovalItemId );
        ApprovalItem = await ApprovalItemAppService.GetAsync(itemId);
    }
}