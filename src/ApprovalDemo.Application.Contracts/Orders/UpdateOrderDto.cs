using System.ComponentModel.DataAnnotations;

namespace ApprovalDemo.Orders;

public class UpdateOrderDto
{
    [Required, MaxLength(200)]
    public string Item { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }

    public bool IsHot { get; set; }
}