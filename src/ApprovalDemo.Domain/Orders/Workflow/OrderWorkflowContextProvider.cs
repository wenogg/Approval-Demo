using System.Threading;
using System.Threading.Tasks;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.Orders.Workflow;

public class OrderWorkflowContextProvider(IRepository<Order, int> orderRepository, ILogger<OrderWorkflowContextProvider> logger)
    : WorkflowContextRefresher<Order>
{
    public override async ValueTask<Order?> LoadAsync(LoadWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var orderId = int.Parse(context.WorkflowInstance.CorrelationId);
        var item = await orderRepository.FindAsync(orderId, cancellationToken: cancellationToken);
        return item;
    }

    public override ValueTask<string?> SaveAsync(SaveWorkflowContext<Order> context, CancellationToken cancellationToken = new CancellationToken())
    {
        logger.LogInformation("OrderWorkflowContextProvider.SaveAsync {Id}", context.Context!.Id);
        return base.SaveAsync(context, cancellationToken);
    }
}