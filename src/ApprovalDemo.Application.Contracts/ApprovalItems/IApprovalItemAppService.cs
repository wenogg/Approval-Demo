using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ApprovalDemo.ApprovalItems;

public interface IApprovalItemAppService : ICrudAppService< //Defines CRUD methods
    ApprovalItemDto, //Used to show books
    int, //Primary key of the book entity
    PagedAndSortedResultRequestDto, //Used for paging/sorting
    UpdateApprovalItemDto>;
