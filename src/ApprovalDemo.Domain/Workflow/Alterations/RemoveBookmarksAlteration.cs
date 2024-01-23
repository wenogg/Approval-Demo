using System.Linq;
using System.Threading.Tasks;
using Elsa.Alterations.Core.Abstractions;
using Elsa.Alterations.Core.Contexts;
using Elsa.Alterations.Core.Contracts;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Filters;

namespace ApprovalDemo.Workflow.Alterations;

/// <summary>
/// Represents an alteration that removes all bookmarks from the workflow instance.
/// </summary>
public class RemoveBookmarksAlteration : IAlteration
{
}

/// <summary>
/// Handles removing all bookmarks from the workflow instance.
/// </summary>
/// <param name="bookmarkManager"></param>
public class RemoveBookmarksAlterationHandler(IBookmarkManager bookmarkManager)
    : AlterationHandlerBase<RemoveBookmarksAlteration>
{

    protected override async ValueTask HandleAsync(AlterationContext context, RemoveBookmarksAlteration alteration)
    {
        var workflowExecutionContext = context.WorkflowExecutionContext;
        var bookmarkIds = workflowExecutionContext
            .Bookmarks
            .Select(s => s.Id)
            .ToList();

        var filter = new BookmarkFilter()
        {
            BookmarkIds = bookmarkIds
        };
        await bookmarkManager.DeleteManyAsync(filter);
    }
}