using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalDemo.Workflow;
using ApprovalDemo.Workflow.Activities;
using Elsa;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowExecutionLogRecords;
using Elsa.Services;
using Elsa.Services.WorkflowStorage;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ApprovalDemo.Orders;

public interface IOrderManager : IDomainService
{
    Task SetStatus(int id, OrderStatusType status);

    Task StartWorkflow(int id);

    Task<List<string>> GetAvailableActions(int id);

    public Task ApplyTransition(int id, string action, string userName);

    Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id);
}

public class OrderManager(
    IRepository<Order, int> orderRepository,
    IWorkflowRegistry workflowRegistry,
    IWorkflowDefinitionDispatcher workflowDispatcher,
    IWorkflowInstanceStore workflowInstanceStore,
    IAuthorizedUserTaskService userTaskService,
    IWorkflowStorageService workflowStorageService,
    IWorkflowTriggerInterruptor workflowTriggerInterruptor,
    IWorkflowExecutionLogStore workflowExecutionLogStore,
    IWorkflowBlueprintInspectorService workflowBlueprintInspectorService,
    IAuthorizationService authorizationService)
    : DomainService, IOrderManager
{
    public IWorkflowBlueprintInspectorService WorkflowBlueprintInspectorService { get; } = workflowBlueprintInspectorService;
    private const string WorkflowBlueprintTag = "Order";

    /// <summary>
    /// Updates the status of the Order
    /// </summary>
    public async Task SetStatus(int id, OrderStatusType status)
    {
        var item = await orderRepository.FindAsync(id);
        if (item == null)
        {
            throw new UserFriendlyException($"Could not find approval item with id {id}");
        }

        item.Status = status;
        await orderRepository.UpdateAsync(item);
    }

    /// <summary>
    /// Initiates a workflow for the Order
    /// </summary>
    public async Task StartWorkflow(int id)
    {
        var item = await orderRepository.FindAsync(id);
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
    /// Returns the available actions for the Order
    /// </summary>
    public async Task<List<string>> GetAvailableActions(int id)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        if (workflowInstance == null)
        {
            return [];
        }

        var actions = new List<string>();
        var userActions = await userTaskService.GetUserActionsAsync(workflowInstance.Id);
        foreach (var action in userActions)
        {
            var hasPermission = await authorizationService.IsGrantedAsync(action.Permission);
            if (hasPermission)
            {
                actions.Add(action.Action);
            }
        }

        return actions;
    }

    /// <summary>
    /// Applies a transition to the Order
    /// </summary>
    public async Task ApplyTransition(int id, string action, string userName)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        var currentActivity = workflowInstance?.BlockingActivities.FirstOrDefault();
        if (currentActivity == null)
        {
            return;
        }

        var input = new AuthorizedUserTaskInput(action, userName);
        await workflowStorageService.UpdateInputAsync(workflowInstance!, new WorkflowInput(input));
        await workflowTriggerInterruptor.InterruptActivityAsync(workflowInstance!, currentActivity.ActivityId);
    }

    /// <summary>
    /// Returns the workflow journal entries for the Order
    /// </summary>
    public async Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id)
    {
        var workflowInstance = await workflowInstanceStore.FindByCorrelationIdAsync(id.ToString());
        if (workflowInstance == null)
        {
            return [];
        }

        var specification = new WorkflowInstanceIdSpecification(workflowInstance.Id);
        var orderBy = OrderBySpecification.OrderBy<WorkflowExecutionLogRecord>(x => x.Timestamp);
        var records = (await workflowExecutionLogStore.FindManyAsync(specification, orderBy)).ToList();
        return records;
    }
}