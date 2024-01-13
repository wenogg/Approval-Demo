using System;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.ApprovalItems.Workflow;

public class ApprovalItemWorkflowContextProvider(IRepository<ApprovalItem, int> approvalItemRepository, ILogger<ApprovalItemWorkflowContextProvider> logger)
    : WorkflowContextRefresher<ApprovalItem>
{
    public override async ValueTask<ApprovalItem?> LoadAsync(LoadWorkflowContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var approvalItemId = int.Parse(context.WorkflowInstance.CorrelationId);
        var item = await approvalItemRepository.FindAsync(approvalItemId, cancellationToken: cancellationToken);
        return item;
    }

    // public override ValueTask<string?> SaveAsync(SaveWorkflowContext<ApprovalItem> context, CancellationToken cancellationToken = new CancellationToken())
    // {
    //     // var item = (await approvalItemRepository.FindAsync(context.Context!.Id, cancellationToken: cancellationToken))!;
    //     // await approvalItemRepository.UpdateAsync(item, cancellationToken: cancellationToken);
    //     return context.Context.Id.ToString();
    // }

    public override ValueTask<string?> SaveAsync(SaveWorkflowContext<ApprovalItem> context, CancellationToken cancellationToken = new CancellationToken())
    {
        logger.LogInformation("ApprovalItemWorkflowContextProvider.SaveAsync {Id}", context.Context!.Id);
        return base.SaveAsync(context, cancellationToken);
    }
}