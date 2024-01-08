using System.Threading.Tasks;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Expressions;
using Elsa.Services;
using Elsa.Providers.WorkflowStorage;
using Elsa.Services.Models;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.ApprovalItems.Workflow;

[Activity(Category = "Approval Items", Description = "Changes the status of the document.")]
public class SetApprovalItemStatusActivity(IApprovalItemManager approvalItemManager, IRepository<ApprovalItem, int> approvalItemRepsoitory) : Activity
{
    [ActivityInput(
        Label = "Approval Item ID",
        Hint = "The ID of the approval item",
        SupportedSyntaxes = [SyntaxNames.JavaScript, SyntaxNames.Liquid]
    )]
    public string ApprovalItemId { get; set; } = default!;

    [ActivityInput(
        Label = "Status",
        Hint = "The new Status of the item",
        SupportedSyntaxes = [SyntaxNames.JavaScript, SyntaxNames.Liquid],
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName
    )]
    public ApprovalStatusType Status { get; set; } = default!;

    [ActivityOutput(
        Hint = "The new status of the item",
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName)]
    public ApprovalStatusType NewStatus { get; set; }

    [ActivityOutput(
        Hint = "The Approval Item",
        DefaultWorkflowStorageProvider = TransientWorkflowStorageProvider.ProviderName)]
    public ApprovalItem Item { get; set; }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var id = int.Parse(ApprovalItemId);
        await approvalItemManager.SetStatus(id, Status);

        Item = (await approvalItemRepsoitory.FindAsync(id))!;
        NewStatus = Item.Status;
        return Done();
    }
}