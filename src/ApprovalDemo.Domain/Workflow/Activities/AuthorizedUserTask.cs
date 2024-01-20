using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;


namespace ApprovalDemo.Workflow.Activities; 

/// <summary>
/// Represents a long running task that requires user input.  The possible outcomes are determined by the available actions.
/// </summary>
[Activity("ApprovalDemo.ApprovalItems.Workflow", "User Tasks", "Triggers when a user action is received")]
[PublicAPI]
public class AuthorizedUserTask : Trigger
{
    [JsonConstructor]
    protected AuthorizedUserTask(string? source = default, int? line = default) : base(source, line)
    {
    }

    /// <summary>
    /// The required permission to invoke an action on this task.
    /// </summary>
    [Description("The permission required to execute this task")]
    public Input<string> Permission { get; set; } = default!;

    /// <summary>
    /// The possible actions that can be taken on this task.
    /// </summary>
    [Input(Description = "The permission required to execute this task", UIHint = InputUIHints.DynamicOutcomes)]
    public Input<List<string>> Actions { get; set; } = default!;

    /// <summary>
    /// The resulting outcome of the task.
    /// </summary>
    [Output] public Output<string>? Result { get; set; }

    /// <summary>
    /// Creates bookmarks for the activity and suspends the workflow.
    /// </summary>
    /// <param name="context"></param>
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        if (context.IsTriggerOfWorkflow())
        {
            // Resume
            await ResumeAsync(context);
        }
        else
        {
            context.CreateBookmarks(GetBookmarkPayloads(context.ExpressionExecutionContext), ResumeAsync);
        }
    }

    /// <summary>
    /// Resumes the task when a user has provided the required input (Action)
    /// </summary>
    private async ValueTask ResumeAsync(ActivityExecutionContext context)
    {
        var logger = context.GetRequiredService<ILogger<AuthorizedUserTask>>();
        var userInput = context.GetWorkflowInput<AuthorizedUserTaskInput>(UserActionBookmarkPayload.InputName);

        logger.LogInformation("Received user action \"{Action}\" from \"{User}\"", userInput.Action, userInput.User);

        // Store webhook payload as output.
        Result.Set(context, userInput.Action);

        // Complete the activity with the outcome.
        await context.CompleteActivityWithOutcomesAsync(userInput.Action);
    }

    protected override object GetTriggerPayload(TriggerIndexingContext context) => GetBookmarkPayload(context.ExpressionExecutionContext);

    private object GetBookmarkPayload(ExpressionExecutionContext context)
    {
        return new UserActionBookmarkPayload("", "");
    }

    /// <summary>
    /// Creates bookmarks for each action available, in order to provide a means to resume when an action occurs.
    /// </summary>
    private IEnumerable<object> GetBookmarkPayloads(ExpressionExecutionContext context)
    {
        // Generate bookmark data for path and selected methods.
        var permission = Permission.GetOrDefault(context) ?? "";
        var actions = Actions.GetOrDefault(context) ?? [];

        return actions
            .Select(x => new UserActionBookmarkPayload(x, permission))
            .Cast<object>()
            .ToArray();
    }
}