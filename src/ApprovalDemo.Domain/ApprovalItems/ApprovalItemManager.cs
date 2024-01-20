using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalDemo.Workflow.Activities;
using Elsa.Common.Models;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Options;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ApprovalDemo.ApprovalItems;

public interface IApprovalItemManager : IDomainService
{
    Task SetStatus(int id, ApprovalStatusType status);

    Task StartWorkflow(int id);

    Task<List<string>> GetAvailableActions(int id);

    public Task ApplyTransition(int id, string action);

    public Task ApplyTransition(int id, string action, string userName);

    // Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id);
}

public class ApprovalItemManager(
    IRepository<ApprovalItem, int> approvalItemRepository,
    IWorkflowDefinitionStore workflowDefinitionStore,
    IWorkflowRuntime workflowRuntime)
    : DomainService, IApprovalItemManager
{
    private const string WorkflowDefinitionName = "Ping";

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

        var workflowDefinition = await FindWorkflowDefinition();

        // Dispatch the workflow.
        await InvokeWorkflow(item, workflowDefinition);
    }


    private async Task InvokeWorkflow(ApprovalItem item, WorkflowDefinition? workflowDefinition)
    {
        var startOptions = new StartWorkflowRuntimeOptions()
        {
            CorrelationId = item.Id.ToString(),
            Input = new Dictionary<string, object>()
            {
                ["item.Id"] = item.Id
            },
            VersionOptions = VersionOptions.Published
        };
        await workflowRuntime.StartWorkflowAsync(workflowDefinition.DefinitionId, startOptions);
    }

    private async Task<WorkflowDefinition?> FindWorkflowDefinition()
    {
        var filter = new WorkflowDefinitionFilter()
        {
            Name = WorkflowDefinitionName,
            VersionOptions = VersionOptions.Published
        };
        var workflowBlueprint = await workflowDefinitionStore.FindAsync(filter);

        if (workflowBlueprint == null)
        {
            throw new UserFriendlyException($"Could not find workflow blueprint with tag {WorkflowDefinitionName}");
        }

        return workflowBlueprint;
    }

    /// <summary>
    /// Returns the available actions for the ApprovalItem
    /// </summary>
    public Task<List<string>> GetAvailableActions(int id)
    {
        return Task.FromResult(new List<string>());
        // var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        // if (workflowInstance == null)
        // {
        //     return [];
        // }
        //
        // List<string> actions = [];
        // foreach (var userTaskService in userTaskServices)
        // {
        //     var userActions = await userTaskService.GetUserActionsAsync(workflowInstance.Id);
        //     actions.AddRange(userActions.Select(x => x.Action));
        // }
        //
        // return actions;
    }

    /// <summary>
    /// Applies a transition to the ApprovalItem
    /// </summary>
    public Task ApplyTransition(int id, string action)
    {
        return Task.CompletedTask;
        // var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        // var currentActivity = workflowInstance?.BlockingActivities.FirstOrDefault();
        // if (currentActivity == null)
        // {
        //     return;
        // }
        // await workflowStorageService.UpdateInputAsync(workflowInstance!, new WorkflowInput(action));
        // await workflowTriggerInterruptor.InterruptActivityAsync(workflowInstance!, currentActivity.ActivityId);
    }

    public Task ApplyTransition(int id, string action, string userName)
    {
        return Task.CompletedTask;
        // var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        // var currentActivity = workflowInstance?.BlockingActivities.FirstOrDefault();
        // if (currentActivity == null)
        // {
        //     return;
        // }
        // var input = new AuthorizedUserTaskInput(action, userName);
        // await workflowStorageService.UpdateInputAsync(workflowInstance!, new WorkflowInput(input));
        // await workflowTriggerInterruptor.InterruptActivityAsync(workflowInstance!, currentActivity.ActivityId);
    }

    /// <summary>
    /// Returns the workflow journal entries for the ApprovalItem
    /// </summary>
    // public async Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id)
    // {
    //     var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
    //     if (workflowInstance == null)
    //     {
    //         return [];
    //     }
    //
    //     var specification = new WorkflowInstanceIdSpecification(workflowInstance.Id);
    //     var totalCount = await workflowExecutionLogStore.CountAsync(specification);
    //     var orderBy = OrderBySpecification.OrderBy<WorkflowExecutionLogRecord>(x => x.Timestamp);
    //     var records = (await workflowExecutionLogStore.FindManyAsync(specification, orderBy)).ToList();
    //     return records;
    // }
}