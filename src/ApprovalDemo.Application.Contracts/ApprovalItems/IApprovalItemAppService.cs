using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalDemo.Workflows;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.ApprovalItems;

public interface
    IApprovalItemAppService : ICrudAppService<ApprovalItemDto, int, PagedAndSortedResultRequestDto,
    UpdateApprovalItemDto>
{
    public Task ApplyTransition(int id, string transition);

    public Task<List<WorkflowVersionDto>> GetWorkflowVersions();
}
