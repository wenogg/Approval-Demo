using Volo.Abp.Domain.Entities.Auditing;

namespace ApprovalDemo.ApprovalItems;

public class ApprovalItem : FullAuditedAggregateRoot<int>
{
    public string Name { get; set; } = default!;

    public ApprovalStatusType Status { get; set; } = ApprovalStatusType.New;
}