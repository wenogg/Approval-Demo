namespace ApprovalDemo.Workflow.Activities;

public class AuthorizedUserTaskInput(string action, string username)
{
    public string Action { get; init; } = action;

    public string User { get; init; } = username;
}