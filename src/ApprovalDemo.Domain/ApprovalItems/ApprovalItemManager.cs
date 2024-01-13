using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elsa;
using Elsa.Activities.UserTask.Contracts;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowExecutionLogRecords;
using Elsa.Services;
using Elsa.Services.WorkflowStorage;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ApprovalDemo.ApprovalItems;

public interface IApprovalItemManager : IDomainService
{
    Task SetStatus(int id, ApprovalStatusType status);

    Task StartWorkflow(int id);

    Task<List<string>> GetAvailableActions(int id);

    public Task ApplyTransition(int id, string transition);

    Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id);
}

public class ApprovalItemManager(
    IRepository<ApprovalItem, int> approvalItemRepository,
    IWorkflowRegistry workflowRegistry,
    IWorkflowDefinitionDispatcher workflowDispatcher,
    IWorkflowInstanceStore workflowInstanceStore,
    IUserTaskService userTaskService,
    IWorkflowStorageService workflowStorageService,
    IWorkflowTriggerInterruptor workflowTriggerInterruptor,
    IWorkflowExecutionLogStore workflowExecutionLogStore)
    : DomainService, IApprovalItemManager
{
    private const string WorkflowBlueprintTag = "ApprovalItem";


    /// <summary>
    /// Updates the status of the ApprovalItem
    /// </summary>
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

    /// <summary>
    /// Initiates a workflow for the ApprovalItem
    /// </summary>
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

    /// <summary>
    /// Returns the available actions for the ApprovalItem
    /// </summary>
    public async Task<List<string>> GetAvailableActions(int id)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        if (workflowInstance == null)
        {
            return [];
        }

        var actions = (await userTaskService.GetUserActionsAsync(workflowInstance.Id)).ToList();
        if (actions.Count == 0)
        {
            return [];
        }

        return actions.Select(x => x.Action).ToList();
    }

    /// <summary>
    /// Applies a transition to the ApprovalItem
    /// </summary>
    public async Task ApplyTransition(int id, string transition)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        var currentActivity = workflowInstance?.BlockingActivities.FirstOrDefault();
        if (currentActivity == null)
        {
            return;
        }
        await workflowStorageService.UpdateInputAsync(workflowInstance!, new WorkflowInput(transition));
        await workflowTriggerInterruptor.InterruptActivityAsync(workflowInstance!, currentActivity.ActivityId);
    }

    /// <summary>
    /// Returns the workflow journal entries for the ApprovalItem
    /// </summary>
    public async Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        if (workflowInstance == null)
        {
            return [];
        }

        var specification = new WorkflowInstanceIdSpecification(workflowInstance.Id);
        var totalCount = await workflowExecutionLogStore.CountAsync(specification);
        var orderBy = OrderBySpecification.OrderBy<WorkflowExecutionLogRecord>(x => x.Timestamp);
        var records = (await workflowExecutionLogStore.FindManyAsync(specification, orderBy)).ToList();
        return records;
    }
}