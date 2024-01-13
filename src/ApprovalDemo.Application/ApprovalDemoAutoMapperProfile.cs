using ApprovalDemo.ApprovalItems;
using AutoMapper;
using Elsa.Models;

namespace ApprovalDemo;

public class ApprovalDemoAutoMapperProfile : Profile
{
    public ApprovalDemoAutoMapperProfile()
    {
        CreateMap<ApprovalItem, ApprovalItemDto>()
            .ReverseMap();

        CreateMap<ApprovalItemDto, UpdateApprovalItemDto>();
        CreateMap<UpdateApprovalItemDto, ApprovalItem>();
        CreateMap<WorkflowExecutionLogRecord, JournalEntryDto>()
            .ForMember(dest => dest.Message, opt => opt.MapFrom(source => source.ActivityType)); ;
    }
}