using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.Orders;

public interface IOrderAppService : ICrudAppService<OrderDto, int, PagedAndSortedResultRequestDto,
    UpdateOrderDto>
{
    public Task ApplyTransition(int id, string transition, string userName);

    public Task ResetWorkflow(int id);

    public Task DetachWorkflow(int id);

}