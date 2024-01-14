using System.Threading.Tasks;
using ApprovalDemo.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace ApprovalDemo.Blazor.Pages.Orders;

public partial class Index
{
    [Inject] private IOrderAppService OrderAppService { get; set; }

    private bool Loading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var hasPermission =
            await AuthorizationService.IsGrantedAsync(Permissions.ApprovalDemoPermissions.Orders.Modify);
        await base.OnInitializedAsync();
    }

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