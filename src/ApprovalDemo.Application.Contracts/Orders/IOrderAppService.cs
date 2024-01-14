using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.Orders;

public interface IOrderAppService : ICrudAppService<OrderDto, int, PagedAndSortedResultRequestDto,
    UpdateOrderDto>;