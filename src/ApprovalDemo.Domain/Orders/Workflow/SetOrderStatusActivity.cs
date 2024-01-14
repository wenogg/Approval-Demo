using System.Threading.Tasks;
using ApprovalDemo.Orders;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Providers.WorkflowStorage;
using Elsa.Services;
using Elsa.Services.Models;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.Orders.Workflow;

[Activity(Category = "Orders", Description = "Changes the status of the Order.")]
public class SetOrderStatusActivity(IOrderManager OrderManager, IRepository<Order, int> OrderRepository) : Activity
{
    [ActivityInput(
        Label = "Approval Item ID",
        Hint = "The ID of the approval item",
        SupportedSyntaxes = [SyntaxNames.JavaScript, SyntaxNames.Liquid]
    )]
    public string OrderId { get; set; } = default!;

    [ActivityInput(
        Label = "Status",
        Hint = "The new Status of the item",
        SupportedSyntaxes = [SyntaxNames.JavaScript, SyntaxNames.Liquid],
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName
    )]
    public OrderStatusType Status { get; set; } = default!;

    [ActivityOutput(
        Hint = "The new status of the item",
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName)]
    public OrderStatusType NewStatus { get; set; }

    [ActivityOutput(
        Hint = "The Approval Item",
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName)]
    public Order Item { get; set; }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var id = int.Parse(OrderId);
        await OrderManager.SetStatus(id, Status);

        Item = (await OrderRepository.FindAsync(id))!;
        NewStatus = Item.Status;
        return Done();
    }
}