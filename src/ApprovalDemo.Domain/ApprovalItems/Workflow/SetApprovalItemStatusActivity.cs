using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Elsa.Expressions.Models;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.ApprovalItems.Workflow;

[Activity("ApprovalDemo.ApprovalItems.Workflow", "Approval Items", "Changes the status of the document")]
[PublicAPI]
public class SetApprovalItemStatusActivity : CodeActivity
{
    [JsonConstructor]
    private SetApprovalItemStatusActivity(string? source = default, int? line = default) : base(source, line)
    {
    }

    public SetApprovalItemStatusActivity(Literal<string> literal, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        ApprovalItemId = new Input<string>(literal);
        Status = new Input<string>(status);
    }

    public SetApprovalItemStatusActivity(Expression expression, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        ApprovalItemId = new Input<string>(expression, new MemoryBlockReference());;
        Status = new Input<string>(status);
    }

    public SetApprovalItemStatusActivity(Input<string> text, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        ApprovalItemId = text;
        Status = new Input<string>(status);
    }

    [Description("The ID of the approval item")]
    public Input<string> ApprovalItemId { get; set; } = default!;

    [Description("The new Status of the item")]
    public Input<string> Status { get; set; } = default!;

    [Output(DisplayName = "The new status of the item")]
    public Output<ApprovalStatusType>? NewStatus { get; set; }

    [Output(DisplayName = "The Approval Item")]
    public Output<ApprovalItem>? Item { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var approvalItemManager = context.GetRequiredService<IApprovalItemManager>();
        var approvalItemRepository = context.GetRequiredService<IRepository<ApprovalItem, int>>();

        var id = int.Parse(context.Get(ApprovalItemId));
        var status = Enum.Parse<ApprovalStatusType>(context.Get(Status));
        await approvalItemManager.SetStatus(id, status);

        var item = (await approvalItemRepository.FindAsync(id))!;
        context.Set(Item, item);
        context.Set(NewStatus, item.Status);
    }
}