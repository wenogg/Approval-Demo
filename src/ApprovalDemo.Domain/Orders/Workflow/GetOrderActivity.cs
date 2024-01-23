using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.Orders.Workflow;


[Activity("ApprovalDemo.Orders.Workflow", "Orders", "Retrieves an order")]
[PublicAPI]
public class GetOrderActivity : CodeActivity
{
    [JsonConstructor]
    private GetOrderActivity(string? source = default, int? line = default) : base(source, line)
    {
    }

    public GetOrderActivity(Literal<string> literal, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = new Input<string>(literal);
    }

    public GetOrderActivity(Expression expression, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = new Input<string>(expression, new MemoryBlockReference());
    }

    public GetOrderActivity(Input<string> text, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default)
        : this(source, line) {
        OrderId = text;
    }

    [Description("The ID of the order")]
    public Input<string> OrderId { get; set; } = default!;

    [Output(DisplayName = "The Order")]
    public Output<Order>? Item { get; set; }

    [Output] public Output<Order>? Result { get; set; }

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var orderManager = context.GetRequiredService<IOrderManager>();
        var orderRepository = context.GetRequiredService<IRepository<Order, int>>();

        var id = int.Parse(context.Get(OrderId).Replace(Order.CorrelationIdPrefix, ""));
        var item = (await orderRepository.FindAsync(id))!;

        Result.Set(context, item);

        context.Set(Item, item);
    }
}