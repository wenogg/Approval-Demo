using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Services;

namespace ApprovalDemo.Workflow.Activities;

public record AuthorizedUserTaskBookmark(string Action, string Permission) : IBookmark;

public class AuthorizedUserTaskBookmarkProvider()
    : BookmarkProvider<AuthorizedUserTaskBookmark, AuthorizedUserTask>
{
    public override async ValueTask<IEnumerable<BookmarkResult>> GetBookmarksAsync(BookmarkProviderContext<AuthorizedUserTask> context, CancellationToken cancellationToken)
    {
        var actions = (await context.ReadActivityPropertyAsync(x => x.Actions, cancellationToken))!;
        var permission = (await context.ReadActivityPropertyAsync(x => x.Permission, cancellationToken))!;
        return actions.Select(x => Result(new AuthorizedUserTaskBookmark(x, permission ?? ""))).ToList();
    }
}