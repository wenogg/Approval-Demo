using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace ApprovalDemo.ApprovalItems;

public class ApprovalItemDto : AuditedEntityDto<int>
{
    public string Name { get; set; }

    public string Description { get; set; }

    public ApprovalStatusType Status { get; set; }

    public List<string> Actions { get; set; } = [];
}