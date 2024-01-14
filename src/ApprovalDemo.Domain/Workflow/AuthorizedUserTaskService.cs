using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApprovalDemo.Workflow.Activities;
using Elsa.Activities.UserTask.Bookmarks;
using Elsa.Activities.UserTask.Contracts;
using Elsa.Activities.UserTask.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications.Bookmarks;
using Elsa.Services;
using Elsa.Services.Models;
using Volo.Abp.DependencyInjection;

namespace ApprovalDemo.Workflow;

public class AuthorizedUserTaskService(
    IBookmarkStore bookmarkStore,
    IBookmarkSerializer bookmarkSerializer) : ITransientDependency, IUserTaskService
{
    public async Task<IEnumerable<UserAction>> GetUserActionsAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        var specification = BookmarkTypeAndWorkflowInstanceSpecification.For<AuthorizedUserTaskBookmark>(workflowInstanceId);
        var bookmarks = await bookmarkStore.FindManyAsync(specification, cancellationToken: cancellationToken);
        var userTaskBookmarks = bookmarks.Select(bookmark => bookmarkSerializer.Deserialize<AuthorizedUserTaskBookmark>(bookmark.Model)).ToList();
        return userTaskBookmarks.Select(x => new UserAction(workflowInstanceId, x.Action));
    }

    public Task<IEnumerable<UserAction>> GetAllUserActionsAsync(int? skip = null, int? take = null, string? tenantId = null,
        CancellationToken cancellationToken = new())
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<CollectedWorkflow>> ExecuteUserActionsAsync(TriggerUserAction taskAction,
        CancellationToken cancellationToken = new())
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<CollectedWorkflow>> DispatchUserActionsAsync(TriggerUserAction taskAction,
        CancellationToken cancellationToken = new())
    {
        throw new System.NotImplementedException();
    }
}