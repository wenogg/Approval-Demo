using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApprovalDemo.Workflow.Activities;
using Elsa.Common.Models;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Entities;
using Elsa.Workflows.Runtime.Filters;
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

    public Task ApplyTransition(int id, string action, string userName);

    public Task<List<WorkflowDefinition>> GetWorkflowVersions();
}

public class ApprovalItemManager(
    IRepository<ApprovalItem, int> approvalItemRepository,
    IWorkflowDefinitionStore workflowDefinitionStore,
    IWorkflowRuntime workflowRuntime,
    IWorkflowInstanceStore workflowInstanceStore,
    IBookmarkStore bookmarkStore)
    : DomainService, IApprovalItemManager
{
    private const string WorkflowDefinitionName = "ApprovalItemWorkflow";

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

        var workflowDefinition = await FindWorkflowDefinition(WorkflowDefinitionName, item.WorkflowDefinitionVersion);
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
    private async Task InvokeWorkflow(ApprovalItem item, WorkflowDefinition workflowDefinition)
    {
        var startOptions = new StartWorkflowRuntimeOptions
        {
            CorrelationId = item.Id.ToString(),
            Input = new Dictionary<string, object>
            {
                ["item.Id"] = item.Id
            },
            VersionOptions = VersionOptions.SpecificVersion(workflowDefinition.Version)
        };
        await workflowRuntime.StartWorkflowAsync(workflowDefinition.DefinitionId, startOptions);
    }

    private async Task<WorkflowDefinition?> FindWorkflowDefinition(string workflowName, int version)
    {
        var filter = new WorkflowDefinitionFilter()
        {
            Name = workflowName,
            VersionOptions = VersionOptions.SpecificVersion(version)
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
    public async Task<List<string>> GetAvailableActions(int id)
    {
        var workflowInstanceFilter = new WorkflowInstanceFilter()
        {
            CorrelationId = id.ToString()
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
            .Cast<UserActionBookmarkPayload>()
            .Select(s => s.Action)
            .ToList();
        return list;
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

    /// <summary>
    /// Applies a transition to the ApprovalItem
    /// </summary>
    public async Task ApplyTransition(int id, string action, string userName)
    {
        var workflowInstanceFilter = new WorkflowInstanceFilter()
        {
            CorrelationId = id.ToString()
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

    public async Task<List<WorkflowDefinition>> GetWorkflowVersions()
    {
        var filter = new WorkflowDefinitionFilter()
        {
            Name = WorkflowDefinitionName,
            VersionOptions = VersionOptions.All
        };
        var items = await workflowDefinitionStore.FindManyAsync(filter);
        return items.OrderByDescending(s => s.Version).ToList();
    }
}