using System;
using System.Linq;
using Elsa.Activities.UserTask.Activities;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Providers.WorkflowStorage;
using Elsa.Serialization;
using Elsa.Services.Models;

namespace ApprovalDemo.Workflow.Activities;


[Trigger(
    Category = "User Tasks",
    Description = "Triggers when a user action is received.",
    Outcomes = new string[0]
)]
public class AuthorizedUserTask(IContentSerializer serializer) : UserTask(serializer)
{
    [ActivityInput(
        Label = "Permission",
        Hint = "The permission required to execute this task",
        SupportedSyntaxes = [SyntaxNames.JavaScript, SyntaxNames.Liquid],
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName
    )]
    public string Permission { get; set; } = default!;

    protected override bool OnCanExecute(ActivityExecutionContext context)
    {
        var input = context.Input as AuthorizedUserTaskInput;
        return input != null && Actions.Contains(input.Action, StringComparer.OrdinalIgnoreCase);
    }

    protected override IActivityExecutionResult OnResume(ActivityExecutionContext context)
    {
        var input = context.Input as AuthorizedUserTaskInput;
        if (input == null)
        {
            return Fault("Invalid input type");
        }

        context.JournalData.Add("user:", input.User);
        context.JournalData.Add("action:", input.Action);

        Output = input.Action;
        return Outcome(input.Action);
    }
}