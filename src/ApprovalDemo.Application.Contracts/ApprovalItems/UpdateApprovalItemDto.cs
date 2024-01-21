using System.ComponentModel.DataAnnotations;

namespace ApprovalDemo.ApprovalItems;

public class UpdateApprovalItemDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public bool IsHot { get; set; }

    [Required]
    public int WorkflowDefinitionVersion { get; set; }

    public ApprovalStatusType Status { get; set; }
}