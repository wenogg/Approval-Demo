using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ApprovalDemo.Orders;

public class OrderAppService(IRepository<Order, int> repository)
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
        return orderDto;
    }

    public override async Task<OrderDto> GetAsync(int id)
    {
        await CheckGetPolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        var item = await MapToGetOutputDtoAsync(entity);
        return item;
    }
}