using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.ApprovalItems;

public interface IApprovalItemAppService : ICrudAppService<ApprovalItemDto, int, PagedAndSortedResultRequestDto, UpdateApprovalItemDto>;
