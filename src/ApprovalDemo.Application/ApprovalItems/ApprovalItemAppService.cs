using System.Collections.Generic;
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

    public Task ApplyTransition(int id, string transition)
    {
        return approvalItemManager.ApplyTransition(id, transition);
    }

    public override async Task<ApprovalItemDto> GetAsync(int id)
    {
        await CheckGetPolicyAsync();

        var entity = await GetEntityByIdAsync(id);

        var item = await MapToGetOutputDtoAsync(entity);
        item.Actions = await approvalItemManager.GetAvailableActions(id);
        // var entries = await approvalItemManager.GetJournalEntries(id);
        // item.Journal = ObjectMapper.Map<List<WorkflowExecutionLogRecord>, List<JournalEntryDto>>(entries);
        return item;
    }
}