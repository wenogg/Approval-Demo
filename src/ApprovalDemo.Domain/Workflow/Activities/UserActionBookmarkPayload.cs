namespace ApprovalDemo.Workflow.Activities;

/// <summary>
/// Represents an action that a user can take to resume a workflow suspended by an AuthorizedUserTask.
/// </summary>
/// <param name="action"></param>
/// <param name="permission"></param>
public class UserActionBookmarkPayload(string action, string permission)
{
    public const string InputName = "UserAction";
    public string Action { get; set; } = action!;

    public string Permission { get; set; } = permission!;
}