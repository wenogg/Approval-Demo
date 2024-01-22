using System.Collections.Generic;
using System.Threading.Tasks;
using ApprovalDemo.ApprovalItems;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.Orders;

public class OrderAppService(IRepository<Order, int> repository, IOrderManager orderManager)
    : CrudAppService<Order, OrderDto, int, PagedAndSortedResultRequestDto, UpdateOrderDto>(
            repository),
        IOrderAppService
{
    protected override string GetListPolicyName => Permissions.ApprovalDemoPermissions.Orders.Default;
    protected override string? CreatePolicyName => Permissions.ApprovalDemoPermissions.Orders.Modify;
    protected override string? DeletePolicyName => Permissions.ApprovalDemoPermissions.Orders.Modify;

    public override async Task<OrderDto> CreateAsync(UpdateOrderDto input)
    {
        var orderDto = await base.CreateAsync(input);
        await orderManager.StartWorkflow(orderDto.Id);
        return orderDto;
    }

    public Task ApplyTransition(int id, string transition, string userName)
    {
        return orderManager.ApplyTransition(id, transition, userName);
    }

    public override async Task<OrderDto> GetAsync(int id)
    {
        await CheckGetPolicyAsync();

        var entity = await GetEntityByIdAsync(id);

        var item = await MapToGetOutputDtoAsync(entity);
        item.Actions = await orderManager.GetAvailableActions(id);
        // var entries = await orderManager.GetJournalEntries(id);
        // item.Journal = ObjectMapper.Map<List<WorkflowExecutionLogRecord>, List<JournalEntryDto>>(entries);
        return item;
    }
}