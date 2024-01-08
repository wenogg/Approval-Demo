using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.ApprovalItems;

public class ApprovalItemAppService(IRepository<ApprovalItem, int> repository, IApprovalItemManager approvalItemManager)
    : CrudAppService<ApprovalItem, ApprovalItemDto, int, PagedAndSortedResultRequestDto, UpdateApprovalItemDto>(
            repository),
        IApprovalItemAppService
{
    public override async Task<ApprovalItemDto> CreateAsync(UpdateApprovalItemDto input)
    {
        var shipmentDto = await base.CreateAsync(input);
        await approvalItemManager.StartWorkflow(shipmentDto.Id);
        return shipmentDto;
    }
}