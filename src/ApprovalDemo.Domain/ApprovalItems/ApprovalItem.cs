using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace ApprovalDemo.ApprovalItems;

public class ApprovalItem : FullAuditedAggregateRoot<int>
{
    [Required, MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    public required string Description { get; set; }

    public ApprovalStatusType Status { get; set; } = ApprovalStatusType.New;

    public bool IsHot { get; set; }
}