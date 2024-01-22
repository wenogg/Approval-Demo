﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalDemo.Workflow;
using ApprovalDemo.Workflow.Activities;
using Elsa.Common.Models;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Entities;
using Elsa.Workflows.Runtime.Filters;
using Elsa.Workflows.Runtime.Options;
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

    // Task<List<WorkflowExecutionLogRecord>> GetJournalEntries(int id);
}

public class OrderManager(
    IRepository<Order, int> orderRepository,
    IAuthorizationService authorizationService,
    IWorkflowDefinitionStore workflowDefinitionStore,
    IWorkflowRuntime workflowRuntime,
    IWorkflowInstanceStore workflowInstanceStore,
    IBookmarkStore bookmarkStore)
    : DomainService, IOrderManager
{
    private const string WorkflowDefinitionName = "OrderWorkflow";

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

        var workflowDefinition = await FindWorkflowDefinition(WorkflowDefinitionName);
        if (workflowDefinition == null)
        {
            throw new UserFriendlyException($"Could not find workflow definition {WorkflowDefinitionName}");
        }

        // Dispatch the workflow.
        await InvokeWorkflow(item, workflowDefinition);
    }

    /// <summary>
    /// Starts the workflow for the ApprovalItem
    /// </summary>
    private async Task InvokeWorkflow(Order item, WorkflowDefinition workflowDefinition)
    {
        var startOptions = new StartWorkflowRuntimeOptions
        {
            CorrelationId = $"{Order.CorrelationIdPrefix}{item.Id}",
            Input = new Dictionary<string, object>
            {
                ["item.Id"] = item.Id
            },
            VersionOptions = VersionOptions.Published
        };
        await workflowRuntime.StartWorkflowAsync(workflowDefinition.DefinitionId, startOptions);
    }

    /// <summary>
    /// Returns the available actions for the Order
    /// </summary>
    public async Task<List<string>> GetAvailableActions(int id)
    {
        var workflowDefinition = await FindWorkflowDefinition(WorkflowDefinitionName);
        if (workflowDefinition == null)
        {
            throw new UserFriendlyException($"Could not find workflow definition {WorkflowDefinitionName}");
        }

        var workflowInstanceFilter = new WorkflowInstanceFilter()
        {
            CorrelationId = $"{Order.CorrelationIdPrefix}{id}",
            DefinitionId = workflowDefinition.DefinitionId
        };
        var workflowInstance = await workflowInstanceStore.FindAsync(workflowInstanceFilter);
        if (workflowInstance == null)
        {
            return [];
        }

        var bookmarks = await GetBookmarks(workflowInstance);
        var list = bookmarks
            .Where(s => s.Payload is UserActionBookmarkPayload)
            .Select(s => s.Payload)
            .Cast<UserActionBookmarkPayload>();

        List<string> actions = [];
        foreach (var action in list)
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
        var workflowInstanceFilter = new WorkflowInstanceFilter()
        {
            CorrelationId = $"{Order.CorrelationIdPrefix}{id}"
        };
        var workflowInstance = await workflowInstanceStore.FindAsync(workflowInstanceFilter);
        if (workflowInstance == null)
        {
            throw new UserFriendlyException($"Could not find workflow instance with correlation id {id}");
        }

        var bookmarks = await GetBookmarks(workflowInstance);
        var bookmarkToResume = bookmarks
            .FirstOrDefault(s =>
                s.Payload is UserActionBookmarkPayload payload
                && payload.Action == action);

        if (bookmarkToResume == null)
        {
            throw new UserFriendlyException($"Could not find bookmark for {action}");
        }

        var userTaskInput = new AuthorizedUserTaskInput(action, userName);
        var inputPayload = new Dictionary<string, object>
        {
            [UserActionBookmarkPayload.InputName] = userTaskInput
        };
        var options = new ResumeWorkflowRuntimeOptions
        {
            BookmarkId = bookmarkToResume.BookmarkId,
            Input = inputPayload
        };
        await workflowRuntime.ResumeWorkflowAsync(workflowInstance.Id, options);
    }

    /// <summary>
    /// Returns the workflow journal entries for the Order
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
    //     var orderBy = OrderBySpecification.OrderBy<WorkflowExecutionLogRecord>(x => x.Timestamp);
    //     var records = (await workflowExecutionLogStore.FindManyAsync(specification, orderBy)).ToList();
    //     return records;
    // }

    private async Task<WorkflowDefinition?> FindWorkflowDefinition(string workflowName)
    {
        var filter = new WorkflowDefinitionFilter()
        {
            Name = workflowName,
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
    /// Retrieves stored bookmarks for a workflow instance
    /// </summary>
    /// <param name="workflowInstance"></param>
    /// <returns></returns>
    private async Task<IEnumerable<StoredBookmark>> GetBookmarks(WorkflowInstance workflowInstance)
    {
        var bookmarkFiler = new BookmarkFilter()
        {
            WorkflowInstanceId = workflowInstance.Id
        };
        var bookmarks = await bookmarkStore.FindManyAsync(bookmarkFiler);
        return bookmarks;
    }
}