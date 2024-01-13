using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace ApprovalDemo.Orders;

public class Order : FullAuditedAggregateRoot<int>
{
    [Required, MaxLength(200)]
    public required string Item { get; set; }

    public required string? Description { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }

    public OrderStatusType Status { get; set; } = OrderStatusType.New;

    public bool IsHot { get; set; }
}