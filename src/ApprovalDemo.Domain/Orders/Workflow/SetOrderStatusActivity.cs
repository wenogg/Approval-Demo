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

namespace ApprovalDemo.Orders.Workflow;

[Activity("ApprovalDemo.Orders.Workflow", "Orders", "Changes the status of the order")]
[PublicAPI]
public class SetOrderStatusActivity : CodeActivity
{
    [JsonConstructor]
    private SetOrderStatusActivity(string? source = default, int? line = default) : base(source, line)
    {
    }

    public SetOrderStatusActivity(Literal<string> literal, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = new Input<string>(literal);
        Status = new Input<string>(status);
    }

    public SetOrderStatusActivity(Expression expression, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = new Input<string>(expression, new MemoryBlockReference());;
        Status = new Input<string>(status);
    }

    public SetOrderStatusActivity(Input<string> text, string status, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = text;
        Status = new Input<string>(status);
    }

    [Description("The ID of the order")]
    public Input<string> OrderId { get; set; } = default!;

    [Description("The new Status of the item")]
    public Input<string> Status { get; set; } = default!;

    [Output(DisplayName = "The new status of the item")]
    public Output<OrderStatusType>? NewStatus { get; set; }

    [Output(DisplayName = "The Order")]
    public Output<Order>? Item { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var orderManager = context.GetRequiredService<IOrderManager>();
        var orderRepository = context.GetRequiredService<IRepository<Order, int>>();

        var id = int.Parse(context.Get(OrderId).Replace(Order.CorrelationIdPrefix, ""));
        var status = Enum.Parse<OrderStatusType>(context.Get(Status));
        await orderManager.SetStatus(id, status);

        var item = (await orderRepository.FindAsync(id))!;
        context.Set(Item, item);
        context.Set(NewStatus, item.Status);
    }
}