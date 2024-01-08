using System.Threading.Tasks;
using ApprovalDemo.ApprovalItems;
using Microsoft.AspNetCore.Components;

namespace ApprovalDemo.Blazor.Pages.ApprovalItem;

public partial class Index
{
    [Inject] private IApprovalItemAppService ApprovalItemAppService { get; set; }

    private bool Loading { get; set; }

    protected override async Task CreateEntityAsync()
    {
        Loading = true;
        try
        {
            await base.CreateEntityAsync();
        }
        finally
        {
            Loading = false;
        }
    }
}