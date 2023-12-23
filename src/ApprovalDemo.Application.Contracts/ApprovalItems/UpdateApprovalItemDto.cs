using System.ComponentModel.DataAnnotations;

namespace ApprovalDemo.ApprovalItems;

public class UpdateApprovalItemDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }
}