using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.ApprovalItems;

public class ApprovalItemAppService(IRepository<ApprovalItem, int> repository)
    : CrudAppService<ApprovalItem, ApprovalItemDto, int, PagedAndSortedResultRequestDto, UpdateApprovalItemDto>(
            repository),
        IApprovalItemAppService
{

}