using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ApprovalDemo.ApprovalItems;

public interface IApprovalItemManager : IDomainService
{
    Task SetStatus(int id, ApprovalStatusType status);

    Task StartWorkflow(int id);
}

public class ApprovalItemManager(
    IRepository<ApprovalItem, int> approvalItemRepository,
    IWorkflowRegistry workflowRegistry,
    IWorkflowDefinitionDispatcher workflowDispatcher)
    : DomainService, IApprovalItemManager
{
    private const string WorkflowBlueprintTag = "ApprovalItem";

    public async Task SetStatus(int id, ApprovalStatusType status)
    {
        var item = await approvalItemRepository.FindAsync(id);
        if (item == null)
        {
            throw new UserFriendlyException($"Could not find approval item with id {id}");
        }

        item.Status = status;
        await approvalItemRepository.UpdateAsync(item);
    }

    public async Task StartWorkflow(int id)
    {
        var item = await approvalItemRepository.FindAsync(id);
        if (item == null)
        {
            throw new UserFriendlyException($"Could not find approval item with id {id}");
        }

        var workflowBlueprint =
            await workflowRegistry.FindByTagAsync(WorkflowBlueprintTag, VersionOptions.Published);

        if (workflowBlueprint == null)
        {
            throw new UserFriendlyException($"Could not find workflow blueprint with tag {WorkflowBlueprintTag}");
        }

        // Dispatch the workflow.
        await workflowDispatcher.DispatchAsync(new ExecuteWorkflowDefinitionRequest(
            workflowBlueprint.Id,
            CorrelationId: item.Id.ToString(),
            Input: new WorkflowInput(item.Id.ToString())));
    }
}