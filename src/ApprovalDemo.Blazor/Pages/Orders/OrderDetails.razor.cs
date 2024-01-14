using System.Threading.Tasks;
using ApprovalDemo.Orders;
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

    protected override async Task OnInitializedAsync()
    {
        Loading = true;
        var itemId = int.Parse(OrderId);
        Order = await OrderAppService.GetAsync(itemId);
        Loading = false;
    }
}