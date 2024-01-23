using System.Threading.Tasks;
using ApprovalDemo.Orders;
using Elsa.Workflows.Api.Endpoints.WorkflowInstances.Journal.GetLastEntry;
using Microsoft.AspNetCore.Components;

namespace ApprovalDemo.Blazor.Pages.Orders;

public partial class OrderDetails
{
    [Parameter]
    public string OrderId { get; set; } = default!;

    [Inject]
    private IOrderAppService OrderAppService { get; set; } = default!;

    private OrderDto Order { get; set; } = new();

    private bool Loading { get; set; }

    private bool CanDetach { get; set; }

    private bool CanReset { get; set; }


    protected override async Task OnInitializedAsync()
    {
        await LoadItem();
    }

    private async Task LoadItem()
    {
        Loading = true;
        var itemId = int.Parse(OrderId);
        Order = await OrderAppService.GetAsync(itemId);
        CanDetach = Order.Status != OrderStatusType.Detached;
        CanReset = Order.Status != OrderStatusType.New && Order.Status != OrderStatusType.Delivered;
        Loading = false;
    }

    private async Task SendUserAction(string action)
    {
        Loading = true;
        var itemId = int.Parse(OrderId);
        await OrderAppService.ApplyTransition(itemId, action, CurrentUser.UserName!);
        Loading = false;

        await LoadItem();
    }

    private async Task ResetWorkflow()
    {
        Loading = true;
        var itemId = int.Parse(OrderId);
        await OrderAppService.ResetWorkflow(itemId);
        Loading = false;

        await LoadItem();
    }

    private async Task DetachWorkflow()
    {
        Loading = true;
        var itemId = int.Parse(OrderId);
        await OrderAppService.DetachWorkflow(itemId);
        Loading = false;

        await LoadItem();
    }
}