using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApprovalDemo.Workflow.Activities;
using Elsa.Persistence;
using Elsa.Persistence.Specifications.Bookmarks;
using Elsa.Services;
using Volo.Abp.DependencyInjection;

namespace ApprovalDemo.Workflow;

public record AuthorizedUserAction(string WorkflowInstanceId, string Action, string Permission);

public interface IAuthorizedUserTaskService
{
    Task<IEnumerable<AuthorizedUserAction>> GetUserActionsAsync(string workflowInstanceId, CancellationToken cancellationToken = default);
}

public class AuthorizedUserTaskService(
    IBookmarkStore bookmarkStore,
    IBookmarkSerializer bookmarkSerializer) : ITransientDependency, IAuthorizedUserTaskService
{
    public async Task<IEnumerable<AuthorizedUserAction>> GetUserActionsAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        var specification = BookmarkTypeAndWorkflowInstanceSpecification.For<AuthorizedUserTaskBookmark>(workflowInstanceId);
        var bookmarks = await bookmarkStore.FindManyAsync(specification, cancellationToken: cancellationToken);
        var userTaskBookmarks = bookmarks.Select(bookmark => bookmarkSerializer.Deserialize<AuthorizedUserTaskBookmark>(bookmark.Model)).ToList();
        return userTaskBookmarks.Select(x => new AuthorizedUserAction(workflowInstanceId, x.Action, x.Permission));
    }
}