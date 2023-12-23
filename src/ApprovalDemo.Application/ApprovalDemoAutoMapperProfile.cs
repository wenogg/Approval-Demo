using ApprovalDemo.ApprovalItems;
using AutoMapper;

namespace ApprovalDemo;

public class ApprovalDemoAutoMapperProfile : Profile
{
    public ApprovalDemoAutoMapperProfile()
    {
        CreateMap<ApprovalItem, ApprovalItemDto>()
            .ReverseMap();

        CreateMap<UpdateApprovalItemDto, ApprovalItem>();
    }
}